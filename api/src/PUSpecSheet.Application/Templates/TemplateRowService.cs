using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

public sealed class TemplateRowService(
    PuSpecSheetDbContext db,
    TableTemplateReader reader,
    TemplateVersionGuard guard) : ITemplateRowService
{
    public async Task<TableTemplateDto> CreateAsync(CreateTemplateRowRequest request, CancellationToken cancellationToken)
    {
        var section = await db.TemplateSections
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.TemplateSectionId)
            .Select(candidate => new
            {
                candidate.TableTemplateVersionId,
                candidate.Name,
                HasChildSections = candidate.ChildSections.Any(),
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (section is null)
        {
            throw new NotFoundException($"Section {request.TemplateSectionId} was not found.");
        }

        await guard.EnsureEditableAsync(section.TableTemplateVersionId, cancellationToken);

        if (section.HasChildSections)
        {
            throw new ConflictException(
                $"\"{section.Name}\" has sections inside it. Rows go in the sections at the bottom level.");
        }

        var rows = await db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => row.TemplateSectionId == request.TemplateSectionId)
            .OrderBy(row => row.DisplayOrder)
            .ToListAsync(cancellationToken);

        var defaultCellTypeId = await DefaultCellType.GetIdAsync(db, cancellationToken);

        var row = new TemplateRow
        {
            TemplateSectionId = request.TemplateSectionId,
            DisplayOrder = rows.Count == 0 ? 1 : rows[^1].DisplayOrder + 1,
            Cells = NewRowCells.For(rows, defaultCellTypeId),
        };

        db.TemplateRows.Add(row);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(section.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var row = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(row, cancellationToken);

        DisplayOrdering.Move(siblings, row, request.DisplayOrder, sibling => sibling.DisplayOrder, SetOrder);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(row.TemplateSection.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var row = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(row, cancellationToken);

        db.TemplateRows.Remove(row);
        DisplayOrdering.Renumber(siblings.Where(sibling => sibling.Id != id), sibling => sibling.DisplayOrder, SetOrder);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(row.TemplateSection.TableTemplateVersionId, cancellationToken);
    }

    private static void SetOrder(TemplateRow row, int order)
    {
        row.DisplayOrder = order;
    }

    private async Task<TemplateRow> FindEditableAsync(int id, CancellationToken cancellationToken)
    {
        var row = await db.TemplateRows
            .Include(candidate => candidate.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (row is null)
        {
            throw new NotFoundException($"Row {id} was not found.");
        }

        await guard.EnsureEditableAsync(row.TemplateSection.TableTemplateVersionId, cancellationToken);
        return row;
    }

    private Task<List<TemplateRow>> LoadSiblingsAsync(TemplateRow row, CancellationToken cancellationToken)
    {
        return db.TemplateRows
            .Where(candidate => candidate.TemplateSectionId == row.TemplateSectionId)
            .ToListAsync(cancellationToken);
    }
}
