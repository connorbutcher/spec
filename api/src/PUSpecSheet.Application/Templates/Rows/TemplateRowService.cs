using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.CellTypes;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates.Rows;

public sealed class TemplateRowService(
    ITemplateSectionRepository sections,
    ITemplateRowRepository rows,
    ITemplateColumnBlockRepository columnBlocks,
    ICellTypeRepository cellTypes,
    IUnitOfWork unitOfWork,
    TableTemplateReader reader,
    TemplateVersionGuard guard) : ITemplateRowService
{
    public async Task<TableTemplateDto> CreateAsync(CreateTemplateRowRequest request, CancellationToken cancellationToken)
    {
        var versionId = await sections.FindVersionIdAsync(request.TemplateSectionId, cancellationToken)
            ?? throw new NotFoundException($"Section {request.TemplateSectionId} was not found.");
        await guard.EnsureEditableAsync(versionId, cancellationToken);

        var sectionRows = await rows.ListInSectionWithCellsForEditAsync(request.TemplateSectionId, cancellationToken);
        var source = CopySource(sectionRows, request.CopyFromRowId);
        var insertIndex = Math.Clamp((request.Position ?? sectionRows.Count + 1) - 1, 0, sectionRows.Count);
        var defaultCellTypeId = await DefaultCellType.GetIdAsync(cellTypes, cancellationToken);
        var columnBlockIds = await columnBlocks.ListIdsAsync(versionId, cancellationToken);

        var row = new TemplateRow
        {
            TemplateSectionId = request.TemplateSectionId,
            Cells = NewRowCells.For(sectionRows, source, insertIndex, defaultCellTypeId, columnBlockIds),
        };

        sectionRows.Insert(insertIndex, row);
        DisplayOrdering.RenumberAsListed(sectionRows, SetOrder);

        rows.Add(row);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(versionId, cancellationToken);
    }

    public async Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var row = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(row, cancellationToken);

        DisplayOrdering.Move(siblings, row, request.DisplayOrder, sibling => sibling.DisplayOrder, SetOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(row.TemplateSection.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var row = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(row, cancellationToken);

        rows.Remove(row);
        DisplayOrdering.Renumber(siblings.Where(sibling => sibling.Id != id), sibling => sibling.DisplayOrder, SetOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(row.TemplateSection.TableTemplateVersionId, cancellationToken);
    }

    private static void SetOrder(TemplateRow row, int order)
    {
        row.DisplayOrder = order;
    }

    private async Task<TemplateRow> FindEditableAsync(int id, CancellationToken cancellationToken)
    {
        var row = await rows.FindForEditAsync(id, cancellationToken);

        if (row is null)
        {
            throw new NotFoundException($"Row {id} was not found.");
        }

        await guard.EnsureEditableAsync(row.TemplateSection.TableTemplateVersionId, cancellationToken);
        return row;
    }

    private Task<List<TemplateRow>> LoadSiblingsAsync(TemplateRow row, CancellationToken cancellationToken)
    {
        return rows.ListInSectionForEditAsync(row.TemplateSectionId, cancellationToken);
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
}
