using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data.Repositories.Templates;

public sealed class TemplateRowRepository(PuSpecSheetDbContext db) : ITemplateRowRepository
{
    public async Task<List<TemplateRow>> ListWithCellsAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => row.TemplateSection.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .Where(row => row.TemplateSection.TableTemplateVersionId == versionId)
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateRow>> ListInSectionWithCellsForEditAsync(int sectionId, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .Include(row => row.Cells)
            .Where(row => row.TemplateSectionId == sectionId)
            .OrderBy(row => row.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateRow>> ListInSectionForEditAsync(int sectionId, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .Where(candidate => candidate.TemplateSectionId == sectionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<TemplateRow?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .Include(candidate => candidate.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<TemplateCellTarget?> FindCellTargetAsync(int rowId, int? columnBlockId, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .AsNoTracking()
            .Where(candidate => candidate.Id == rowId)
            .Select(candidate => new TemplateCellTarget(
                candidate.TemplateSection.TableTemplateVersionId,
                candidate.Cells
                    .Where(cell => cell.TemplateColumnBlockId == columnBlockId)
                    .Max(cell => (int?)(cell.Column + cell.ColumnSpan)) ?? 1))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public void Add(TemplateRow row)
    {
        db.TemplateRows.Add(row);
    }

    public void Remove(TemplateRow row)
    {
        db.TemplateRows.Remove(row);
    }
}
