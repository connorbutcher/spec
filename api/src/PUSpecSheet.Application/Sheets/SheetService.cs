using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

public sealed class SheetService(
    PuSpecSheetDbContext db,
    SheetReader reader,
    SheetPublisher publisher,
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
        await publisher.PublishAsync(sheetId, request.Note, cancellationToken);
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

        await db.RowRevisionsOf(sheetId).DraftsOf(me).ExecuteDeleteAsync(cancellationToken);
        await db.ColumnBlockRevisionsOf(sheetId).DraftsOf(me).ExecuteDeleteAsync(cancellationToken);
        await db.SectionRevisionsOf(sheetId).DraftsOf(me).ExecuteDeleteAsync(cancellationToken);
        await db.TableRevisionsOf(sheetId).DraftsOf(me).ExecuteDeleteAsync(cancellationToken);

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
}
