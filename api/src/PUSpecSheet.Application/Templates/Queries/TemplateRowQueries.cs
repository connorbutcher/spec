using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Templates;

namespace PUSpecSheet.Application.Templates.Queries;

/// <summary>The rows of template sections.</summary>
public static class TemplateRowQueries
{
    /// <summary>Every row of a version with its cells, read only.</summary>
    public static async Task<List<TemplateRow>> ListWithCellsAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => row.TemplateSection.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The ids of every row of a version.</summary>
    public static async Task<List<int>> ListIdsAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .Where(row => row.TemplateSection.TableTemplateVersionId == versionId)
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>A section's rows with their cells, in display order, tracked for editing.</summary>
    public static async Task<List<TemplateRow>> ListInSectionWithCellsForEditAsync(PuSpecSheetDbContext db, int sectionId, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .Include(row => row.Cells)
            .Where(row => row.TemplateSectionId == sectionId)
            .OrderBy(row => row.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>A section's rows, tracked for editing.</summary>
    public static async Task<List<TemplateRow>> ListInSectionForEditAsync(PuSpecSheetDbContext db, int sectionId, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .Where(candidate => candidate.TemplateSectionId == sectionId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The row with its section, tracked for editing; null when there is no such row.</summary>
    public static async Task<TemplateRow?> FindForEditAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .Include(candidate => candidate.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    /// <summary>Where a new cell in the row (and column block, if any) goes; null when there is no such row.</summary>
    public static async Task<TemplateCellTarget?> FindCellTargetAsync(PuSpecSheetDbContext db, int rowId, int? columnBlockId, CancellationToken cancellationToken)
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

    /// <summary>The row with its cells, read only, when it is in the template section; null otherwise.</summary>
    public static async Task<TemplateRow?> FindInSectionWithCellsAsync(PuSpecSheetDbContext db, int id, int sectionId, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .SingleOrDefaultAsync(row => row.Id == id && row.TemplateSectionId == sectionId, cancellationToken);
    }

    /// <summary>Every row of the versions with its cells, read only.</summary>
    public static async Task<List<TemplateRow>> ListWithCellsInVersionsAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken)
    {
        return await db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => versionIds.Contains(row.TemplateSection.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
    }
}
