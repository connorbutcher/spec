using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public sealed class TemplateRowDataService : ITemplateRowDataService
{
    private readonly PuSpecSheetDbContext _db;

    public TemplateRowDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<DbTemplateRow>> ListWithCellsAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => row.TemplateSection.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateRows
            .Where(row => row.TemplateSection.TableTemplateVersionId == versionId)
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbTemplateRow>> ListInSectionWithCellsForEditAsync(int sectionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateRows
            .Include(row => row.Cells)
            .Where(row => row.TemplateSectionId == sectionId)
            .OrderBy(row => row.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbTemplateRow>> ListInSectionForEditAsync(int sectionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateRows
            .Where(candidate => candidate.TemplateSectionId == sectionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<DbTemplateRow?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.TemplateRows
            .Include(candidate => candidate.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<TemplateCellTarget?> FindCellTargetAsync(int rowId, int? columnBlockId, CancellationToken cancellationToken)
    {
        return await _db.TemplateRows
            .AsNoTracking()
            .Where(candidate => candidate.Id == rowId)
            .Select(candidate => new TemplateCellTarget
            {
                TableTemplateVersionId = candidate.TemplateSection.TableTemplateVersionId,
                NextColumn = candidate.Cells
                    .Where(cell => cell.TemplateColumnBlockId == columnBlockId)
                    .Max(cell => (int?)(cell.Column + cell.ColumnSpan)) ?? 1,
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<List<DbTemplateRow>> ListWithCellsInVersionsAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken)
    {
        return await _db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => versionIds.Contains(row.TemplateSection.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
    }
}
