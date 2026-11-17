using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public sealed class TemplateCellDataService : ITemplateCellDataService
{
    private readonly PuSpecSheetDbContext _db;

    public TemplateCellDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<DbTemplateCell?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.TemplateCells
            .Include(candidate => candidate.TemplateRow)
            .ThenInclude(row => row.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<DbTemplateCell?> FindWithTypeForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.TemplateCells
            .Include(candidate => candidate.CellType)
            .Include(candidate => candidate.TemplateRow)
            .ThenInclude(row => row.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<bool> ColumnTakenAsync(int rowId, int? columnBlockId, int column, int exceptId, CancellationToken cancellationToken)
    {
        return await _db.TemplateCells.AnyAsync(
            other => other.TemplateRowId == rowId
                && other.TemplateColumnBlockId == columnBlockId
                && other.Column == column
                && other.Id != exceptId,
            cancellationToken);
    }

    public async Task ShiftRightAsync(int rowId, int? columnBlockId, int fromColumn, CancellationToken cancellationToken)
    {
        await _db.TemplateCells
            .Where(cell => cell.TemplateRowId == rowId
                && cell.TemplateColumnBlockId == columnBlockId
                && cell.Column >= fromColumn)
            .ExecuteUpdateAsync(setters => setters.SetProperty(cell => cell.Column, cell => cell.Column + 1), cancellationToken);
    }

    public async Task DeleteInColumnBlockAsync(int columnBlockId, CancellationToken cancellationToken)
    {
        await _db.TemplateCells
            .Where(cell => cell.TemplateColumnBlockId == columnBlockId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<List<DbTemplateCell>> ListAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await _db.TemplateCells.AsNoTracking().Where(cell => ids.Contains(cell.Id)).ToListAsync(cancellationToken);
    }

    public async Task<bool> LookupKeyExistsAsync(string key, CancellationToken cancellationToken)
    {
        return await _db.TemplateCells.AnyAsync(cell => cell.LookupKey == key, cancellationToken);
    }
}
