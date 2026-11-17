using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public sealed class SheetColumnBlockDataService : ISheetColumnBlockDataService
{
    private readonly PuSpecSheetDbContext _db;

    public SheetColumnBlockDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<DbSheetColumnBlock>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _db.SheetColumnBlocks
            .AsNoTracking()
            .Include(block => block.TemplateColumnBlock)
            .Where(block => block.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    public async Task<DbSheetColumnBlock?> FindWithTemplateAndTableAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.SheetColumnBlocks
            .AsNoTracking()
            .Include(block => block.TemplateColumnBlock)
            .Include(block => block.SheetTable)
            .SingleOrDefaultAsync(block => block.Id == id, cancellationToken);
    }

    public async Task<List<SheetColumnBlockLink>> ListLinksOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await _db.SheetColumnBlocks
            .AsNoTracking()
            .Where(block => block.SheetTableId == tableId)
            .Select(block => new SheetColumnBlockLink { Id = block.Id, TemplateColumnBlockId = block.TemplateColumnBlockId })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await _db.SheetColumnBlocks
            .Where(candidate => candidate.SheetTableId == tableId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbSheetColumnBlockRevision>> ListVisibleOnTableAsync(int tableId, int userId, CancellationToken cancellationToken)
    {
        return await _db.SheetColumnBlockRevisions
            .Where(revision => revision.SheetColumnBlock.SheetTableId == tableId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken)
    {
        return SheetRevisionQueries.ListHoldsAsync(_db.SheetColumnBlockRevisions.Where(revision => revision.SheetColumnBlock.SheetTableId == tableId), userId, cancellationToken);
    }

    public async Task DeleteAsync(IReadOnlyCollection<int> blockIds, CancellationToken cancellationToken)
    {
        if (blockIds.Count == 0)
        {
            return;
        }

        await _db.TextValues.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await _db.NumericValues.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await _db.DateValues.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await _db.BooleanValues.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await _db.OptionValues.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await _db.LabelledTextValues.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await _db.CellSettings.Where(value => value.SheetCell.SheetColumnBlockId != null && blockIds.Contains(value.SheetCell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await _db.SheetCells.Where(cell => cell.SheetColumnBlockId != null && blockIds.Contains(cell.SheetColumnBlockId.Value)).ExecuteDeleteAsync(cancellationToken);
        await _db.SheetColumnBlocks.Where(block => blockIds.Contains(block.Id)).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<List<DbSheetColumnBlockRevision>> ListCurrentOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _db.SheetColumnBlockRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == sheetId)
            .Current()
            .Where(revision => !revision.IsDeleted)
            .Include(revision => revision.SheetColumnBlock)
            .ToListAsync(cancellationToken);
    }

    public async Task SupersedeCurrentAsync(IReadOnlyCollection<int> columnBlockIds, DateTime nowUtc, CancellationToken cancellationToken)
    {
        await _db.SheetColumnBlockRevisions.Where(revision => columnBlockIds.Contains(revision.SheetColumnBlockId)).SupersedeCurrentAsync(nowUtc, cancellationToken);
    }
}
