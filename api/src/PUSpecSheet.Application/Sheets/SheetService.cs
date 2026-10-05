using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

public sealed class SheetService(
    PuSpecSheetDbContext db,
    SheetReader reader,
    RowValueStore valueStore,
    DraftSweeper sweeper,
    ICurrentUser currentUser) : ISheetService
{
    public async Task<SheetDto> OpenAsync(int phaseId, int sheetTypeId, SheetViewPoint view, CancellationToken cancellationToken)
    {
        var sheetId = await db.Sheets
            .Where(sheet => sheet.PhaseId == phaseId && sheet.SheetTypeId == sheetTypeId)
            .Select(sheet => (int?)sheet.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (sheetId is null)
        {
            sheetId = await CreateSheetAsync(phaseId, sheetTypeId, cancellationToken);
        }

        if (view.IsLive)
        {
            await sweeper.SweepAsync(sheetId.Value, cancellationToken);
        }

        return await reader.ReadAsync(sheetId.Value, view, cancellationToken);
    }

    public async Task<SheetDto> GetAsync(int sheetId, SheetViewPoint view, CancellationToken cancellationToken)
    {
        if (view.IsLive)
        {
            await sweeper.SweepAsync(sheetId, cancellationToken);
        }

        return await reader.ReadAsync(sheetId, view, cancellationToken);
    }

    public async Task<SheetDto> PublishAsync(int sheetId, PublishSheetRequest request, CancellationToken cancellationToken)
    {
        var sheet = await db.Sheets.SingleOrDefaultAsync(candidate => candidate.Id == sheetId, cancellationToken)
            ?? throw new NotFoundException($"Sheet {sheetId} was not found.");
        await sweeper.SweepAsync(sheetId, cancellationToken);
        var me = currentUser.UserId;

        var tableDrafts = await db.SheetTableRevisions
            .Where(revision => revision.SheetTable.SheetId == sheetId && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me)
            .ToListAsync(cancellationToken);
        var sectionDrafts = await db.SheetSectionRevisions
            .Where(revision => revision.SheetSection.SheetTable.SheetId == sheetId && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me)
            .ToListAsync(cancellationToken);
        var rowDrafts = await db.SheetRowRevisions
            .Where(revision => revision.SheetRow.SheetSection.SheetTable.SheetId == sheetId && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me)
            .ToListAsync(cancellationToken);

        var blockDrafts = await db.SheetColumnBlockRevisions
            .Where(revision => revision.SheetColumnBlock.SheetTable.SheetId == sheetId && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me)
            .ToListAsync(cancellationToken);

        if (tableDrafts.Count + sectionDrafts.Count + rowDrafts.Count + blockDrafts.Count == 0)
        {
            throw new InvalidRequestException("You have no changes to publish.");
        }

        await EnsureRequiredCellsFilledAsync(rowDrafts, cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var lastVersion = await db.SheetVersions
            .Where(version => version.SheetId == sheetId)
            .MaxAsync(version => (int?)version.VersionNumber, cancellationToken) ?? 0;
        var version = new SheetVersion
        {
            SheetId = sheetId,
            VersionNumber = lastVersion + 1,
            PublishedAtUtc = now,
            PublishedByUserId = me,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };
        db.SheetVersions.Add(version);

        // Two people publishing at once would both claim the next number; the sheet's row version makes
        // the second one fail instead. Touching the sheet is what makes the check happen.
        db.Entry(sheet).State = EntityState.Modified;

        // The replaced revisions close their range at the moment the new ones open, in a separate save so
        // the "one current revision per item" index never sees two at once.
        var tableIds = tableDrafts.Select(draft => draft.SheetTableId).ToList();
        var sectionIds = sectionDrafts.Select(draft => draft.SheetSectionId).ToList();
        var rowIds = rowDrafts.Select(draft => draft.SheetRowId).ToList();
        var blockIds = blockDrafts.Select(draft => draft.SheetColumnBlockId).ToList();

        var previousTables = await db.SheetTableRevisions
            .Where(revision => tableIds.Contains(revision.SheetTableId) && revision.Status == RevisionStatus.Published && revision.SupersededAtUtc == null)
            .ToListAsync(cancellationToken);
        var previousSections = await db.SheetSectionRevisions
            .Where(revision => sectionIds.Contains(revision.SheetSectionId) && revision.Status == RevisionStatus.Published && revision.SupersededAtUtc == null)
            .ToListAsync(cancellationToken);
        var previousRows = await db.SheetRowRevisions
            .Where(revision => rowIds.Contains(revision.SheetRowId) && revision.Status == RevisionStatus.Published && revision.SupersededAtUtc == null)
            .ToListAsync(cancellationToken);

        var previousBlocks = await db.SheetColumnBlockRevisions
            .Where(revision => blockIds.Contains(revision.SheetColumnBlockId) && revision.Status == RevisionStatus.Published && revision.SupersededAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var previous in previousTables)
        {
            previous.SupersededAtUtc = now;
        }

        foreach (var previous in previousSections)
        {
            previous.SupersededAtUtc = now;
        }

        foreach (var previous in previousRows)
        {
            previous.SupersededAtUtc = now;
        }

        foreach (var previous in previousBlocks)
        {
            previous.SupersededAtUtc = now;
        }

        await db.SaveSheetChangesAsync(cancellationToken);

        foreach (var draft in tableDrafts)
        {
            Publish(draft, version, now);
        }

        foreach (var draft in sectionDrafts)
        {
            Publish(draft, version, now);
        }

        foreach (var draft in rowDrafts)
        {
            Publish(draft, version, now);
        }

        foreach (var draft in blockDrafts)
        {
            Publish(draft, version, now);
        }

        await db.SaveSheetChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> DiscardDraftsAsync(int sheetId, CancellationToken cancellationToken)
    {
        if (!await db.Sheets.AnyAsync(sheet => sheet.Id == sheetId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetId} was not found.");
        }

        var me = currentUser.UserId;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        await db.SheetRowRevisions
            .Where(revision => revision.SheetRow.SheetSection.SheetTable.SheetId == sheetId && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me)
            .ExecuteDeleteAsync(cancellationToken);
        await db.SheetColumnBlockRevisions
            .Where(revision => revision.SheetColumnBlock.SheetTable.SheetId == sheetId && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me)
            .ExecuteDeleteAsync(cancellationToken);
        await db.SheetSectionRevisions
            .Where(revision => revision.SheetSection.SheetTable.SheetId == sheetId && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me)
            .ExecuteDeleteAsync(cancellationToken);
        await db.SheetTableRevisions
            .Where(revision => revision.SheetTable.SheetId == sheetId && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me)
            .ExecuteDeleteAsync(cancellationToken);

        // Items that only ever existed as the user's drafts now have no revisions at all, so they go too.
        var orphanBlockIds = await db.SheetColumnBlocks
            .Where(block => block.SheetTable.SheetId == sheetId && !block.Revisions.Any())
            .Select(block => block.Id)
            .ToListAsync(cancellationToken);
        await ColumnBlockCells.DeleteBlocksAsync(db, orphanBlockIds, cancellationToken);
        await db.SheetRows
            .Where(row => row.SheetSection.SheetTable.SheetId == sheetId && !row.Revisions.Any())
            .ExecuteDeleteAsync(cancellationToken);
        await db.SheetSections
            .Where(section => section.SheetTable.SheetId == sheetId && !section.Revisions.Any())
            .ExecuteDeleteAsync(cancellationToken);
        await db.SheetTables
            .Where(table => table.SheetId == sheetId && !table.Revisions.Any())
            .ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private async Task<int> CreateSheetAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        var available = await db.PhaseSheetTypes
            .AnyAsync(link => link.PhaseId == phaseId && link.SheetTypeId == sheetTypeId, cancellationToken);
        if (!available)
        {
            throw new NotFoundException("That sheet type isn't available to that phase.");
        }

        var sheet = new Sheet { PhaseId = phaseId, SheetTypeId = sheetTypeId };
        db.Sheets.Add(sheet);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return sheet.Id;
        }
        catch (DbUpdateException)
        {
            // Someone opened the same sheet at the same moment and created it first.
            db.Entry(sheet).State = EntityState.Detached;
            return await db.Sheets
                .Where(candidate => candidate.PhaseId == phaseId && candidate.SheetTypeId == sheetTypeId)
                .Select(candidate => candidate.Id)
                .SingleAsync(cancellationToken);
        }
    }

    private static void Publish(ISheetRevision draft, SheetVersion version, DateTime now)
    {
        draft.Status = RevisionStatus.Published;
        draft.PublishedAtUtc = now;
        draft.SheetVersionId = version.Id;
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
        var empty = cells.Count(cell =>
            !(values.GetValueOrDefault(revisionByRow[cell.SheetRowId])?.ContainsKey(cell.Id) ?? false));

        if (empty > 0)
        {
            throw new InvalidRequestException(
                $"{empty} required {(empty == 1 ? "cell is" : "cells are")} still empty. Fill them in, or discard the new rows, before publishing.");
        }
    }
}
