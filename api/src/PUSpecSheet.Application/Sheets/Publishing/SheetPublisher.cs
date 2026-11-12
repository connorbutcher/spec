using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Application.Users.Queries;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Approvals;
using PUSpecSheet.Database.Entities.Sheets;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Publishing;

/// <summary>
/// Publishes drafts on a sheet as its next version: the current user's own, leaving other people's as they
/// are, or every draft on the sheet. Each draft becomes its item's current published revision and the
/// revision it replaces is closed at the same moment.
/// <para>
/// A draft published for someone else keeps them as its author, so the sheet still says whose change it
/// was; the version says who published it. Their lock goes with the draft, as it does for the publisher.
/// A draft of theirs that changes nothing is dropped rather than published, like the publisher's own. If
/// they save into a draft while it is being published, the publish fails as a conflict and nothing changes.
/// </para>
/// <para>
/// Through the API a publish first waits for approval: <see cref="PrepareAsync"/> gathers and checks the
/// drafts for a submission, and <see cref="PublishSubmissionAsync"/> publishes them once it is approved.
/// </para>
/// </summary>
public sealed class SheetPublisher
{
    private readonly PuSpecSheetDbContext db;
    private readonly SheetDraftSetLoader draftLoader;
    private readonly DraftSweeper sweeper;
    private readonly ICurrentUser currentUser;
    private readonly TimeProvider clock;

    public SheetPublisher(
        PuSpecSheetDbContext db,
        SheetDraftSetLoader draftLoader,
        DraftSweeper sweeper,
        ICurrentUser currentUser,
        TimeProvider clock)
    {
        this.db = db;
        this.draftLoader = draftLoader;
        this.sweeper = sweeper;
        this.currentUser = currentUser;
        this.clock = clock;
    }

    /// <summary>Publishes straight away, without approval. Only start-up seeding does this.</summary>
    /// <exception cref="NotFoundException">The sheet doesn't exist.</exception>
    /// <exception cref="InvalidRequestException">There is nothing to publish, or a required cell is empty.</exception>
    /// <exception cref="ConflictException">Someone else published the sheet at the same moment.</exception>
    public async Task PublishAsync(int sheetId, string? note, PublishScope scope, CancellationToken cancellationToken)
    {
        var drafts = await PrepareAsync(sheetId, scope, cancellationToken);
        var sheet = await FindSheetAsync(sheetId, cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await PublishDraftsAsync(sheet, drafts, note, currentUser.UserId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// The drafts a publish in <paramref name="scope"/> would take, once those that change nothing are swept
    /// away, checked to be publishable. Drafts already waiting for approval are never among them.
    /// </summary>
    /// <exception cref="NotFoundException">The sheet doesn't exist.</exception>
    /// <exception cref="InvalidRequestException">There is nothing to publish, or a required cell is empty.</exception>
    public async Task<SheetDraftSet> PrepareAsync(int sheetId, PublishScope scope, CancellationToken cancellationToken)
    {
        if (!await SheetQueries.ExistsAsync(db, sheetId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetId} was not found.");
        }

        await sweeper.SweepAsync(sheetId, scope, cancellationToken);
        var drafts = await draftLoader.LoadAsync(sheetId, new ScopeDraftSelection(scope, currentUser.UserId), cancellationToken);
        if (drafts.Count == 0)
        {
            throw new InvalidRequestException(scope == PublishScope.All ? "There are no changes to publish." : "You have no changes to publish.");
        }

        await EnsureRequiredCellsFilledAsync(drafts.Rows, cancellationToken);
        return drafts;
    }

    /// <summary>
    /// Publishes an approved submission's drafts as the sheet's next version, attributed to its submitter and
    /// carrying its note, and records the version on it. Changes already made to the tracked submission are
    /// saved in the same transaction, so the submission and the version can't disagree.
    /// </summary>
    /// <exception cref="ConflictException">Someone else published the sheet, or decided the submission, at the same moment.</exception>
    public async Task<SheetVersion> PublishSubmissionAsync(SheetSubmission submission, CancellationToken cancellationToken)
    {
        var sheet = await FindSheetAsync(submission.SheetId, cancellationToken);
        var drafts = await draftLoader.LoadAsync(sheet.Id, new SubmissionDraftSelection(submission.Id), cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var version = await PublishDraftsAsync(sheet, drafts, submission.Note, submission.SubmittedByUserId, cancellationToken);
        submission.SheetVersionId = version.Id;
        await db.SaveSheetChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return version;
    }

    private async Task<Sheet> FindSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await SheetQueries.FindForEditAsync(db, sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");
    }

    /// <summary>Publishes the drafts as the sheet's next version, inside the caller's transaction.</summary>
    private async Task<SheetVersion> PublishDraftsAsync(
        Sheet sheet,
        SheetDraftSet drafts,
        string? note,
        int publishedByUserId,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var lastVersion = await SheetQueries.GetLastVersionNumberAsync(db, sheet.Id, cancellationToken);
        var version = AddNextVersion(sheet, note, now, publishedByUserId, lastVersion);

        // The replaced revisions close their range at the moment the new ones open, before the drafts are
        // published, so the "one current revision per item" index never sees two at once. If the version
        // save below fails, the transaction undoes them.
        await SupersedeReplacedAsync(drafts, now, cancellationToken);
        await db.SaveSheetChangesAsync(cancellationToken);

        foreach (var draft in drafts.All)
        {
            draft.Status = RevisionStatus.Published;
            draft.PublishedAtUtc = now;
            draft.SheetVersionId = version.Id;
        }

        await db.SaveSheetChangesAsync(cancellationToken);
        return version;
    }

    /// <summary>Adds the sheet's next version, numbered one after its last.</summary>
    private SheetVersion AddNextVersion(Sheet sheet, string? note, DateTime now, int publishedByUserId, int lastVersion)
    {
        var version = new SheetVersion
        {
            SheetId = sheet.Id,
            VersionNumber = lastVersion + 1,
            PublishedAtUtc = now,
            PublishedByUserId = publishedByUserId,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
        };
        db.SheetVersions.Add(version);

        // Two people publishing at once would both claim the next number; the sheet's row version makes
        // the second one fail instead. Touching the sheet is what makes the check happen.
        db.MarkModified(sheet);
        return version;
    }


    /// <summary>Closes the published revisions the drafts replace, one statement per kind of item.</summary>
    private async Task SupersedeReplacedAsync(SheetDraftSet drafts, DateTime now, CancellationToken cancellationToken)
    {
        var tableIds = drafts.Tables.Select(draft => draft.SheetTableId).ToList();
        var sectionIds = drafts.Sections.Select(draft => draft.SheetSectionId).ToList();
        var rowIds = drafts.Rows.Select(draft => draft.SheetRowId).ToList();
        var columnBlockIds = drafts.ColumnBlocks.Select(draft => draft.SheetColumnBlockId).ToList();
        await db.SheetTableRevisions.Where(revision => tableIds.Contains(revision.SheetTableId)).SupersedeCurrentAsync(now, cancellationToken);
        await db.SheetSectionRevisions.Where(revision => sectionIds.Contains(revision.SheetSectionId)).SupersedeCurrentAsync(now, cancellationToken);
        await db.SheetRowRevisions.Where(revision => rowIds.Contains(revision.SheetRowId)).SupersedeCurrentAsync(now, cancellationToken);
        await db.SheetColumnBlockRevisions.Where(revision => columnBlockIds.Contains(revision.SheetColumnBlockId)).SupersedeCurrentAsync(now, cancellationToken);
    }

    private async Task EnsureRequiredCellsFilledAsync(IReadOnlyList<SheetRowRevision> rowDrafts, CancellationToken cancellationToken)
    {
        var live = rowDrafts.Where(draft => !draft.IsDeleted).ToList();
        if (live.Count == 0)
        {
            return;
        }

        var emptyRowIds = await RowsWithEmptyRequiredCellsAsync(live, cancellationToken);
        if (emptyRowIds.Count > 0)
        {
            await ThrowRequiredCellsEmptyAsync(live, emptyRowIds, cancellationToken);
        }
    }

    /// <summary>The row of each required cell left empty in the drafts, once per cell.</summary>
    private async Task<List<int>> RowsWithEmptyRequiredCellsAsync(List<SheetRowRevision> live, CancellationToken cancellationToken)
    {
        var rowIds = live.Select(draft => draft.SheetRowId).ToList();
        var required = await SheetCellQueries.ListRequiredAsync(db, rowIds, cancellationToken);
        var toFill = required.Where(cell => cell.Kind.StoresValue() && cell.Kind != CellKind.Checkbox).ToList();
        if (toFill.Count == 0)
        {
            return [];
        }

        var held = await RowValueQueries.LoadAsync(db, live.Select(draft => draft.Id).ToList(), cancellationToken);
        var revisionByRow = live.ToDictionary(draft => draft.SheetRowId, draft => draft.Id);
        return toFill
            .Where(cell => !(held.GetValueOrDefault(revisionByRow[cell.SheetRowId])?.GetValueOrDefault(cell.Id)?.HasValue ?? false))
            .Select(cell => cell.SheetRowId)
            .ToList();
    }

    private async Task ThrowRequiredCellsEmptyAsync(List<SheetRowRevision> live, List<int> emptyRowIds, CancellationToken cancellationToken)
    {
        var empty = emptyRowIds.Count;
        var stillEmpty = $"{empty} required {(empty == 1 ? "cell is" : "cells are")} still empty";
        var me = currentUser.UserId;
        var authorByRow = live.ToDictionary(draft => draft.SheetRowId, draft => draft.AuthorUserId);
        var otherAuthorIds = emptyRowIds.Select(rowId => authorByRow[rowId]).Where(author => author != me).Distinct().ToList();
        if (otherAuthorIds.Count == 0)
        {
            throw new InvalidRequestException($"{stillEmpty}. Fill them in, or discard the new rows, before publishing.");
        }

        // The publisher can't fill in or discard someone else's rows, so the way out is different.
        var names = await UserQueries.GetSortedDisplayNamesAsync(db, otherAuthorIds, cancellationToken);
        throw new InvalidRequestException(
            $"{stillEmpty}, in rows being edited by {string.Join(", ", names)}. Publish only your own changes, or ask them to fill those in first.");
    }
}
