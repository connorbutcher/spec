using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

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
/// </summary>
public sealed class SheetPublisher(
    PuSpecSheetDbContext db,
    RowValueStore valueStore,
    DraftSweeper sweeper,
    ICurrentUser currentUser)
{
    /// <exception cref="NotFoundException">The sheet doesn't exist.</exception>
    /// <exception cref="InvalidRequestException">There is nothing to publish, or a required cell is empty.</exception>
    /// <exception cref="ConflictException">Someone else published the sheet at the same moment.</exception>
    public async Task PublishAsync(int sheetId, string? note, PublishScope scope, CancellationToken cancellationToken)
    {
        var sheet = await db.Sheets.SingleOrDefaultAsync(candidate => candidate.Id == sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");
        await sweeper.SweepAsync(sheetId, scope, cancellationToken);
        var me = currentUser.UserId;

        var tableDrafts = await db.TableRevisionsOf(sheetId).DraftsIn(scope, me).ToListAsync(cancellationToken);
        var sectionDrafts = await db.SectionRevisionsOf(sheetId).DraftsIn(scope, me).ToListAsync(cancellationToken);
        var rowDrafts = await db.RowRevisionsOf(sheetId).DraftsIn(scope, me).ToListAsync(cancellationToken);
        var columnBlockDrafts = await db.ColumnBlockRevisionsOf(sheetId).DraftsIn(scope, me).ToListAsync(cancellationToken);
        List<ISheetRevision> drafts = [.. tableDrafts, .. sectionDrafts, .. rowDrafts, .. columnBlockDrafts];
        if (drafts.Count == 0)
        {
            throw new InvalidRequestException(scope == PublishScope.All ? "There are no changes to publish." : "You have no changes to publish.");
        }

        await EnsureRequiredCellsFilledAsync(rowDrafts, cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var version = await AddNextVersionAsync(sheet, note, now, cancellationToken);

        // The replaced revisions close their range at the moment the new ones open, in a separate save so
        // the "one current revision per item" index never sees two at once.
        var tableIds = tableDrafts.Select(draft => draft.SheetTableId).ToList();
        var sectionIds = sectionDrafts.Select(draft => draft.SheetSectionId).ToList();
        var rowIds = rowDrafts.Select(draft => draft.SheetRowId).ToList();
        var columnBlockIds = columnBlockDrafts.Select(draft => draft.SheetColumnBlockId).ToList();
        await SupersedeAsync(db.SheetTableRevisions.Where(revision => tableIds.Contains(revision.SheetTableId)), now, cancellationToken);
        await SupersedeAsync(db.SheetSectionRevisions.Where(revision => sectionIds.Contains(revision.SheetSectionId)), now, cancellationToken);
        await SupersedeAsync(db.SheetRowRevisions.Where(revision => rowIds.Contains(revision.SheetRowId)), now, cancellationToken);
        await SupersedeAsync(db.SheetColumnBlockRevisions.Where(revision => columnBlockIds.Contains(revision.SheetColumnBlockId)), now, cancellationToken);
        await db.SaveSheetChangesAsync(cancellationToken);

        foreach (var draft in drafts)
        {
            draft.Status = RevisionStatus.Published;
            draft.PublishedAtUtc = now;
            draft.SheetVersionId = version.Id;
        }

        await db.SaveSheetChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>Adds the sheet's next version, numbered one after its last.</summary>
    private async Task<SheetVersion> AddNextVersionAsync(Sheet sheet, string? note, DateTime now, CancellationToken cancellationToken)
    {
        var lastVersion = await db.SheetVersions
            .Where(version => version.SheetId == sheet.Id)
            .MaxAsync(version => (int?)version.VersionNumber, cancellationToken) ?? 0;
        var version = new SheetVersion
        {
            SheetId = sheet.Id,
            VersionNumber = lastVersion + 1,
            PublishedAtUtc = now,
            PublishedByUserId = currentUser.UserId,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
        };
        db.SheetVersions.Add(version);

        // Two people publishing at once would both claim the next number; the sheet's row version makes
        // the second one fail instead. Touching the sheet is what makes the check happen.
        db.Entry(sheet).State = EntityState.Modified;
        return version;
    }

    /// <summary>Closes the current published revisions among <paramref name="revisions"/>; the save comes later.</summary>
    private static async Task SupersedeAsync<TRevision>(IQueryable<TRevision> revisions, DateTime now, CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        foreach (var previous in await revisions.Current().ToListAsync(cancellationToken))
        {
            previous.SupersededAtUtc = now;
        }
    }

    private async Task EnsureRequiredCellsFilledAsync(IReadOnlyList<SheetRowRevision> rowDrafts, CancellationToken cancellationToken)
    {
        var live = rowDrafts.Where(draft => !draft.IsDeleted).ToList();
        if (live.Count == 0)
        {
            return;
        }

        var rowIds = live.Select(draft => draft.SheetRowId).ToList();
        var cells = await db.SheetCells
            .AsNoTracking()
            .Include(cell => cell.TemplateCell)
            .ThenInclude(templateCell => templateCell.CellType)
            .Where(cell => rowIds.Contains(cell.SheetRowId) && cell.TemplateCell.IsRequired)
            .ToListAsync(cancellationToken);
        cells = cells.Where(cell => cell.TemplateCell.CellType.Kind.StoresValue() && cell.TemplateCell.CellType.Kind != CellKind.Checkbox).ToList();
        if (cells.Count == 0)
        {
            return;
        }

        var values = await valueStore.LoadAsync(live.Select(draft => draft.Id).ToList(), cancellationToken);
        var revisionByRow = live.ToDictionary(draft => draft.SheetRowId, draft => draft.Id);
        var emptyRowIds = cells
            .Where(cell => !(values.GetValueOrDefault(revisionByRow[cell.SheetRowId])?.ContainsKey(cell.Id) ?? false))
            .Select(cell => cell.SheetRowId)
            .ToList();
        if (emptyRowIds.Count == 0)
        {
            return;
        }

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
        var names = await db.Users
            .Where(user => otherAuthorIds.Contains(user.Id))
            .OrderBy(user => user.DisplayName)
            .Select(user => user.DisplayName)
            .ToListAsync(cancellationToken);
        throw new InvalidRequestException(
            $"{stillEmpty}, in rows being edited by {string.Join(", ", names)}. Publish only your own changes, or ask them to fill those in first.");
    }
}
