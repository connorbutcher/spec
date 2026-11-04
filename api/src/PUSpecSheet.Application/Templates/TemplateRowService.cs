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
        var versionId = await db.TemplateSections
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.TemplateSectionId)
            .Select(candidate => (int?)candidate.TableTemplateVersionId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Section {request.TemplateSectionId} was not found.");
        await guard.EnsureEditableAsync(versionId, cancellationToken);

        var rows = await db.TemplateRows
            .Include(row => row.Cells)
            .Where(row => row.TemplateSectionId == request.TemplateSectionId)
            .OrderBy(row => row.DisplayOrder)
            .ToListAsync(cancellationToken);
        var source = CopySource(rows, request.CopyFromRowId);
        var insertIndex = Math.Clamp((request.Position ?? rows.Count + 1) - 1, 0, rows.Count);
        var defaultCellTypeId = await DefaultCellType.GetIdAsync(db, cancellationToken);
        var columnBlockIds = await ColumnBlockIdsAsync(versionId, cancellationToken);

        var row = new TemplateRow
        {
            TemplateSectionId = request.TemplateSectionId,
            Cells = NewRowCells.For(rows, source, insertIndex, defaultCellTypeId, columnBlockIds),
        };

        rows.Insert(insertIndex, row);
        DisplayOrdering.RenumberAsListed(rows, SetOrder);

        db.TemplateRows.Add(row);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(versionId, cancellationToken);
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

    /// <summary>The row a new row copies its cells from: the one asked for, or the section's last.</summary>
    private static TemplateRow? CopySource(List<TemplateRow> rows, int? copyFromRowId)
    {
        if (copyFromRowId is not int copyFromId)
        {
            return rows.LastOrDefault();
        }

        return rows.SingleOrDefault(candidate => candidate.Id == copyFromId)
            ?? throw new InvalidRequestException($"Row {copyFromId} isn't in this section.");
    }

    private async Task<List<int>> ColumnBlockIdsAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateColumnBlocks
            .Where(block => block.TableTemplateVersionId == versionId)
            .OrderBy(block => block.DisplayOrder)
            .Select(block => block.Id)
            .ToListAsync(cancellationToken);
    }
}
