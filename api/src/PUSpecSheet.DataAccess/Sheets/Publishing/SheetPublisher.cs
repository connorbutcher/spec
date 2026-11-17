using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Approvals;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Publishing;

// Publishes drafts on a sheet as its next version: the current user's own, leaving other people's as they are, or
// every draft on the sheet. Each draft becomes its item's current published revision and the revision it replaces is
// closed at the same moment. A draft published for someone else keeps them as its author, so the sheet still says
// whose change it was; the version says who published it. Their lock goes with the draft, as it does for the
// publisher. A draft of theirs that changes nothing is dropped rather than published, like the publisher's own. If
// they save into a draft while it is being published, the publish fails as a conflict and nothing changes. Through
// the API a publish first waits for approval: PrepareAsync gathers and checks the drafts for a submission, and
// PublishSubmissionAsync publishes them once it is approved.
public sealed class SheetPublisher
{
    private readonly PuSpecSheetDbContext _db;
    private readonly SheetDraftSetLoader _draftLoader;
    private readonly DraftSweeper _sweeper;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _clock;
    private readonly IUserDataService _userData;
    private readonly ISheetDataService _sheetData;
    private readonly ISheetCellDataService _sheetCellData;
    private readonly ICellValueDataService _cellValueData;

    public SheetPublisher(
        PuSpecSheetDbContext db,
        SheetDraftSetLoader draftLoader,
        DraftSweeper sweeper,
        ICurrentUser currentUser,
        TimeProvider clock,
        IUserDataService userData,
        ISheetDataService sheetData,
        ISheetCellDataService sheetCellData,
        ICellValueDataService cellValueData)
    {
        _db = db;
        _draftLoader = draftLoader;
        _sweeper = sweeper;
        _currentUser = currentUser;
        _clock = clock;
        _userData = userData;
        _sheetData = sheetData;
        _sheetCellData = sheetCellData;
        _cellValueData = cellValueData;
    }

    public async Task PublishAsync(int sheetId, string? note, PublishScope scope, CancellationToken cancellationToken)
    {
        var drafts = await PrepareAsync(sheetId, scope, cancellationToken);
        var sheet = await FindSheetAsync(sheetId, cancellationToken);
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        await PublishDraftsAsync(sheet, drafts, note, _currentUser.UserId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    // The drafts a publish in scope would take, once those that change nothing are swept away, checked to be
    // publishable. Drafts already waiting for approval are never among them.
    public async Task<SheetDraftSet> PrepareAsync(int sheetId, PublishScope scope, CancellationToken cancellationToken)
    {
        if (!await _sheetData.ExistsAsync(sheetId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetId} was not found.");
        }

        await _sweeper.SweepAsync(sheetId, scope, cancellationToken);
        var drafts = await _draftLoader.LoadAsync(sheetId, DraftSelection.InScope(scope, _currentUser.UserId), cancellationToken);
        if (drafts.Count == 0)
        {
            throw new InvalidRequestException(scope == PublishScope.All ? "There are no changes to publish." : "You have no changes to publish.");
        }

        await EnsureRequiredCellsFilledAsync(drafts.Rows, cancellationToken);
        return drafts;
    }

    // Publishes an approved submission's drafts as the sheet's next version, attributed to its submitter and carrying
    // its note, and records the version on it. Changes already made to the tracked submission are saved in the same
    // transaction, so the submission and the version can't disagree.
    public async Task<DbSheetVersion> PublishSubmissionAsync(DbSheetSubmission submission, CancellationToken cancellationToken)
    {
        var sheet = await FindSheetAsync(submission.SheetId, cancellationToken);
        var drafts = await _draftLoader.LoadAsync(sheet.Id, DraftSelection.InSubmission(submission.Id), cancellationToken);
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var version = await PublishDraftsAsync(sheet, drafts, submission.Note, submission.SubmittedByUserId, cancellationToken);
        submission.SheetVersionId = version.Id;
        await _db.SaveSheetChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return version;
    }

    private async Task<DbSheet> FindSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _sheetData.FindForEditAsync(sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");
    }

    private async Task<DbSheetVersion> PublishDraftsAsync(
        DbSheet sheet,
        SheetDraftSet drafts,
        string? note,
        int publishedByUserId,
        CancellationToken cancellationToken)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        var lastVersion = await _sheetData.GetLastVersionNumberAsync(sheet.Id, cancellationToken);
        var version = AddNextVersion(sheet, note, now, publishedByUserId, lastVersion);

        // The replaced revisions close their range at the moment the new ones open, before the drafts are
        // published, so the "one current revision per item" index never sees two at once. If the version
        // save below fails, the transaction undoes them.
        await SupersedeReplacedAsync(drafts, now, cancellationToken);
        await _db.SaveSheetChangesAsync(cancellationToken);

        foreach (var draft in drafts.All)
        {
            draft.Status = RevisionStatus.Published;
            draft.PublishedAtUtc = now;
            draft.SheetVersionId = version.Id;
        }

        await _db.SaveSheetChangesAsync(cancellationToken);
        return version;
    }

    private DbSheetVersion AddNextVersion(DbSheet sheet, string? note, DateTime now, int publishedByUserId, int lastVersion)
    {
        var version = new DbSheetVersion
        {
            SheetId = sheet.Id,
            VersionNumber = lastVersion + 1,
            PublishedAtUtc = now,
            PublishedByUserId = publishedByUserId,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
        };
        _db.SheetVersions.Add(version);

        // Two people publishing at once would both claim the next number; the sheet's row version makes
        // the second one fail instead. Touching the sheet is what makes the check happen.
        _db.MarkModified(sheet);
        return version;
    }

    private async Task SupersedeReplacedAsync(SheetDraftSet drafts, DateTime now, CancellationToken cancellationToken)
    {
        var tableIds = drafts.Tables.Select(draft => draft.SheetTableId).ToList();
        var sectionIds = drafts.Sections.Select(draft => draft.SheetSectionId).ToList();
        var rowIds = drafts.Rows.Select(draft => draft.SheetRowId).ToList();
        var columnBlockIds = drafts.ColumnBlocks.Select(draft => draft.SheetColumnBlockId).ToList();
        await _db.SheetTableRevisions.Where(revision => tableIds.Contains(revision.SheetTableId)).SupersedeCurrentAsync(now, cancellationToken);
        await _db.SheetSectionRevisions.Where(revision => sectionIds.Contains(revision.SheetSectionId)).SupersedeCurrentAsync(now, cancellationToken);
        await _db.SheetRowRevisions.Where(revision => rowIds.Contains(revision.SheetRowId)).SupersedeCurrentAsync(now, cancellationToken);
        await _db.SheetColumnBlockRevisions.Where(revision => columnBlockIds.Contains(revision.SheetColumnBlockId)).SupersedeCurrentAsync(now, cancellationToken);
    }

    private async Task EnsureRequiredCellsFilledAsync(IReadOnlyList<DbSheetRowRevision> rowDrafts, CancellationToken cancellationToken)
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

    private async Task<List<int>> RowsWithEmptyRequiredCellsAsync(List<DbSheetRowRevision> live, CancellationToken cancellationToken)
    {
        var rowIds = live.Select(draft => draft.SheetRowId).ToList();
        var required = await _sheetCellData.ListRequiredAsync(rowIds, cancellationToken);
        var toFill = required.Where(cell => cell.Kind.StoresValue() && cell.Kind != CellKind.Checkbox).ToList();
        if (toFill.Count == 0)
        {
            return [];
        }

        var held = await _cellValueData.LoadAsync(live.Select(draft => draft.Id).ToList(), cancellationToken);
        var revisionByRow = live.ToDictionary(draft => draft.SheetRowId, draft => draft.Id);
        return toFill
            .Where(cell => !(held.GetValueOrDefault(revisionByRow[cell.SheetRowId])?.GetValueOrDefault(cell.Id)?.HasValue ?? false))
            .Select(cell => cell.SheetRowId)
            .ToList();
    }

    private async Task ThrowRequiredCellsEmptyAsync(List<DbSheetRowRevision> live, List<int> emptyRowIds, CancellationToken cancellationToken)
    {
        var empty = emptyRowIds.Count;
        var stillEmpty = $"{empty} required {(empty == 1 ? "cell is" : "cells are")} still empty";
        var me = _currentUser.UserId;
        var authorByRow = live.ToDictionary(draft => draft.SheetRowId, draft => draft.AuthorUserId);
        var otherAuthorIds = emptyRowIds.Select(rowId => authorByRow[rowId]).Where(author => author != me).Distinct().ToList();
        if (otherAuthorIds.Count == 0)
        {
            throw new InvalidRequestException($"{stillEmpty}. Fill them in, or remove the new rows, before publishing.");
        }

        // The publisher can't fill in or remove someone else's rows, so the way out is different.
        var names = await _userData.GetSortedDisplayNamesAsync(otherAuthorIds, cancellationToken);
        throw new InvalidRequestException(
            $"{stillEmpty}, in rows being edited by {string.Join(", ", names)}. Publish only your own changes, or ask them to fill those in first.");
    }
}
