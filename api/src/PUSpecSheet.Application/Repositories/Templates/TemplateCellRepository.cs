using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Database.Entities.Templates;

using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Repositories.Templates;

public sealed class TemplateCellRepository : ITemplateCellRepository
{
    private readonly PuSpecSheetDbContext db;

    public TemplateCellRepository(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<TemplateCell?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await db.TemplateCells
            .Include(candidate => candidate.TemplateRow)
            .ThenInclude(row => row.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<TemplateCell?> FindWithTypeForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await db.TemplateCells
            .Include(candidate => candidate.CellType)
            .Include(candidate => candidate.TemplateRow)
            .ThenInclude(row => row.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<bool> ColumnTakenAsync(int rowId, int? columnBlockId, int column, int exceptId, CancellationToken cancellationToken)
    {
        return await db.TemplateCells.AnyAsync(
            other => other.TemplateRowId == rowId
                && other.TemplateColumnBlockId == columnBlockId
                && other.Column == column
                && other.Id != exceptId,
            cancellationToken);
    }

    public async Task ShiftRightAsync(int rowId, int? columnBlockId, int fromColumn, CancellationToken cancellationToken)
    {
        await db.TemplateCells
            .Where(cell => cell.TemplateRowId == rowId
                && cell.TemplateColumnBlockId == columnBlockId
                && cell.Column >= fromColumn)
            .ExecuteUpdateAsync(setters => setters.SetProperty(cell => cell.Column, cell => cell.Column + 1), cancellationToken);
    }

    public async Task DeleteInColumnBlockAsync(int columnBlockId, CancellationToken cancellationToken)
    {
        await db.TemplateCells
            .Where(cell => cell.TemplateColumnBlockId == columnBlockId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<List<TemplateCell>> ListAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await db.TemplateCells.AsNoTracking().Where(cell => ids.Contains(cell.Id)).ToListAsync(cancellationToken);
    }

    public async Task<bool> LookupKeyExistsAsync(string key, CancellationToken cancellationToken)
    {
        return await db.TemplateCells.AnyAsync(cell => cell.LookupKey == key, cancellationToken);
    }

    public void Add(TemplateCell cell)
    {
        db.TemplateCells.Add(cell);
    }

    public void Remove(TemplateCell cell)
    {
        db.TemplateCells.Remove(cell);
    }
}
