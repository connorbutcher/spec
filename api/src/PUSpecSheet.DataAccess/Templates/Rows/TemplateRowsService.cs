using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.CellTypes;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Templates.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates.Rows;

public sealed class TemplateRowsService : ITemplateRowsService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly TableTemplateReader _reader;
    private readonly TemplateVersionGuard _guard;
    private readonly ICellTypeDataService _cellTypes;

    public TemplateRowsService(
        PuSpecSheetDbContext db,
        TableTemplateReader reader,
        TemplateVersionGuard guard,
        ICellTypeDataService cellTypes)
    {
        _db = db;
        _reader = reader;
        _guard = guard;
        _cellTypes = cellTypes;
    }

    public async Task<TableTemplateDto> CreateAsync(CreateTemplateRowRequest request, CancellationToken cancellationToken)
    {
        var versionId = await TemplateSectionQueries.FindVersionIdAsync(_db, request.TemplateSectionId, cancellationToken)
            ?? throw new NotFoundException($"Section {request.TemplateSectionId} was not found.");
        await _guard.EnsureEditableAsync(versionId, cancellationToken);

        var sectionRows = await TemplateRowQueries.ListInSectionWithCellsForEditAsync(_db, request.TemplateSectionId, cancellationToken);
        var source = CopySource(sectionRows, request.CopyFromRowId);
        var insertIndex = Math.Clamp((request.Position ?? sectionRows.Count + 1) - 1, 0, sectionRows.Count);
        var defaultCellTypeId = await _cellTypes.GetDefaultIdAsync(cancellationToken);
        var columnBlockIds = await TemplateColumnBlockQueries.ListIdsAsync(_db, versionId, cancellationToken);

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

    public async Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var row = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(row, cancellationToken);

        DisplayOrdering.Move(siblings, row, request.DisplayOrder, sibling => sibling.DisplayOrder, SetOrder);
        await _db.SaveChangesAsync(cancellationToken);

        return await _reader.ReadVersionAsync(row.TemplateSection.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
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
        var row = await TemplateRowQueries.FindForEditAsync(_db, id, cancellationToken);

        if (row is null)
        {
            throw new NotFoundException($"Row {id} was not found.");
        }

        await _guard.EnsureEditableAsync(row.TemplateSection.TableTemplateVersionId, cancellationToken);
        return row;
    }

    private Task<List<DbTemplateRow>> LoadSiblingsAsync(DbTemplateRow row, CancellationToken cancellationToken)
    {
        return TemplateRowQueries.ListInSectionForEditAsync(_db, row.TemplateSectionId, cancellationToken);
    }

    /// <summary>The row a new row copies its cells from: the one asked for, or the section's last.</summary>
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
