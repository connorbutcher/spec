using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.CellTypes;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates.Rows;

public sealed class TemplateRowsService : ITemplateRowsService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly TableTemplateReader _reader;
    private readonly TemplateVersionGuard _guard;
    private readonly ICellTypeDataService _cellTypes;
    private readonly ITemplateSectionDataService _templateSectionData;
    private readonly ITemplateRowDataService _templateRowData;
    private readonly ITemplateColumnBlockDataService _templateColumnBlockData;

    public TemplateRowsService(
        PuSpecSheetDbContext db,
        TableTemplateReader reader,
        TemplateVersionGuard guard,
        ICellTypeDataService cellTypes,
        ITemplateSectionDataService templateSectionData,
        ITemplateRowDataService templateRowData,
        ITemplateColumnBlockDataService templateColumnBlockData)
    {
        _db = db;
        _reader = reader;
        _guard = guard;
        _cellTypes = cellTypes;
        _templateSectionData = templateSectionData;
        _templateRowData = templateRowData;
        _templateColumnBlockData = templateColumnBlockData;
    }

    public async Task<TableTemplate> CreateAsync(CreateTemplateRowRequest request, CancellationToken cancellationToken)
    {
        var versionId = await _templateSectionData.FindVersionIdAsync(request.TemplateSectionId, cancellationToken)
            ?? throw new NotFoundException($"Section {request.TemplateSectionId} was not found.");
        await _guard.EnsureEditableAsync(versionId, cancellationToken);

        var sectionRows = await _templateRowData.ListInSectionWithCellsForEditAsync(request.TemplateSectionId, cancellationToken);
        var source = CopySource(sectionRows, request.CopyFromRowId);
        var insertIndex = Math.Clamp((request.Position ?? sectionRows.Count + 1) - 1, 0, sectionRows.Count);
        var defaultCellTypeId = await _cellTypes.GetDefaultIdAsync(cancellationToken);
        var columnBlockIds = await _templateColumnBlockData.ListIdsAsync(versionId, cancellationToken);

        var row = new DbTemplateRow
        {
            TemplateSectionId = request.TemplateSectionId,
            Cells = NewRowCells.For(sectionRows, source, insertIndex, defaultCellTypeId, columnBlockIds),
        };

        sectionRows.Insert(insertIndex, row);
        DisplayOrdering.RenumberAsListed(sectionRows, SetOrder);

        _db.TemplateRows.Add(row);
        await _db.SaveChangesAsync(cancellationToken);

        return await _reader.ReadVersionAsync(versionId, cancellationToken);
    }

    public async Task<TableTemplate> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var row = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(row, cancellationToken);

        DisplayOrdering.Move(siblings, row, request.DisplayOrder, sibling => sibling.DisplayOrder, SetOrder);
        await _db.SaveChangesAsync(cancellationToken);

        return await _reader.ReadVersionAsync(row.TemplateSection.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplate> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var row = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(row, cancellationToken);

        _db.TemplateRows.Remove(row);
        DisplayOrdering.Renumber(siblings.Where(sibling => sibling.Id != id), sibling => sibling.DisplayOrder, SetOrder);
        await _db.SaveChangesAsync(cancellationToken);

        return await _reader.ReadVersionAsync(row.TemplateSection.TableTemplateVersionId, cancellationToken);
    }

    private static void SetOrder(DbTemplateRow row, int order)
    {
        row.DisplayOrder = order;
    }

    private async Task<DbTemplateRow> FindEditableAsync(int id, CancellationToken cancellationToken)
    {
        var row = await _templateRowData.FindForEditAsync(id, cancellationToken);

        if (row is null)
        {
            throw new NotFoundException($"Row {id} was not found.");
        }

        await _guard.EnsureEditableAsync(row.TemplateSection.TableTemplateVersionId, cancellationToken);
        return row;
    }

    private Task<List<DbTemplateRow>> LoadSiblingsAsync(DbTemplateRow row, CancellationToken cancellationToken)
    {
        return _templateRowData.ListInSectionForEditAsync(row.TemplateSectionId, cancellationToken);
    }

    private static DbTemplateRow? CopySource(List<DbTemplateRow> rows, int? copyFromRowId)
    {
        if (copyFromRowId is not int copyFromId)
        {
            return rows.LastOrDefault();
        }

        return rows.SingleOrDefault(candidate => candidate.Id == copyFromId)
            ?? throw new InvalidRequestException($"Row {copyFromId} isn't in this section.");
    }
}
