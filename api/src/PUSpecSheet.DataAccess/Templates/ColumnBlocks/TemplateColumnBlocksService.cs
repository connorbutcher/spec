using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.CellTypes;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Templates.Sections;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates.ColumnBlocks;

public sealed class TemplateColumnBlocksService : ITemplateColumnBlocksService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly TableTemplateReader _reader;
    private readonly TemplateVersionGuard _guard;
    private readonly ICellTypeDataService _cellTypes;
    private readonly ITableTemplateDataService _tableTemplateData;
    private readonly ITemplateRowDataService _templateRowData;
    private readonly ITemplateCellDataService _templateCellData;
    private readonly ITemplateColumnBlockDataService _templateColumnBlockData;

    public TemplateColumnBlocksService(
        PuSpecSheetDbContext db,
        TableTemplateReader reader,
        TemplateVersionGuard guard,
        ICellTypeDataService cellTypes,
        ITableTemplateDataService tableTemplateData,
        ITemplateRowDataService templateRowData,
        ITemplateCellDataService templateCellData,
        ITemplateColumnBlockDataService templateColumnBlockData)
    {
        _db = db;
        _reader = reader;
        _guard = guard;
        _cellTypes = cellTypes;
        _tableTemplateData = tableTemplateData;
        _templateRowData = templateRowData;
        _templateCellData = templateCellData;
        _templateColumnBlockData = templateColumnBlockData;
    }

    public async Task<TableTemplate> CreateAsync(
        CreateTemplateColumnBlockRequest request,
        CancellationToken cancellationToken)
    {
        var versionId = request.TableTemplateVersionId;
        await _guard.EnsureEditableAsync(versionId, cancellationToken);
        await EnsureHorizontalAsync(versionId, cancellationToken);

        var lastOrder = await _templateColumnBlockData.GetLastDisplayOrderAsync(versionId, cancellationToken);

        var rowIds = await _templateRowData.ListIdsAsync(versionId, cancellationToken);

        var cellTypeId = await _cellTypes.GetDefaultIdAsync(cancellationToken);

        // A block shows in every row, so each row starts with one cell in it.
        var block = new DbTemplateColumnBlock
        {
            TableTemplateVersionId = versionId,
            Name = request.Name.Trim(),
            DisplayOrder = (lastOrder ?? 0) + 1,
            MinInstances = 0,
            MaxInstances = null,
            InitialInstances = 1,
            Cells = rowIds
                .Select(rowId => new DbTemplateCell { TemplateRowId = rowId, Column = 1, CellTypeId = cellTypeId })
                .ToList(),
        };

        _db.TemplateColumnBlocks.Add(block);
        await _db.SaveChangesAsync(cancellationToken);
        return await _reader.ReadVersionAsync(versionId, cancellationToken);
    }

    public async Task<TableTemplate> UpdateAsync(
        int id,
        UpdateTemplateColumnBlockRequest request,
        CancellationToken cancellationToken)
    {
        var block = await FindEditableAsync(id, cancellationToken);
        SectionInstanceRules.EnsureValid(request.MinInstances, request.MaxInstances, request.InitialInstances);

        block.Name = request.Name.Trim();
        block.MinInstances = request.MinInstances;
        block.MaxInstances = request.MaxInstances;
        block.InitialInstances = request.InitialInstances;
        block.StickyColumnCount = request.StickyColumnCount;

        await _db.SaveChangesAsync(cancellationToken);
        return await _reader.ReadVersionAsync(block.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplate> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var block = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(block, cancellationToken);

        DisplayOrdering.Move(siblings, block, request.DisplayOrder, sibling => sibling.DisplayOrder, SetOrder);
        await _db.SaveChangesAsync(cancellationToken);

        return await _reader.ReadVersionAsync(block.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplate> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var block = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(block, cancellationToken);

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        // The block's cells can't cascade from it (they already cascade from their rows), so they go first.
        await _templateCellData.DeleteInColumnBlockAsync(id, cancellationToken);

        _db.TemplateColumnBlocks.Remove(block);
        DisplayOrdering.Renumber(siblings.Where(sibling => sibling.Id != id), sibling => sibling.DisplayOrder, SetOrder);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await _reader.ReadVersionAsync(block.TableTemplateVersionId, cancellationToken);
    }

    private static void SetOrder(DbTemplateColumnBlock block, int order)
    {
        block.DisplayOrder = order;
    }

    private async Task EnsureHorizontalAsync(int versionId, CancellationToken cancellationToken)
    {
        var orientation = await _tableTemplateData.GetOrientationAsync(versionId, cancellationToken);

        if (orientation != TemplateOrientation.Horizontal)
        {
            throw new ConflictException("Only horizontal tables have column blocks. Make the table horizontal first.");
        }
    }

    private async Task<DbTemplateColumnBlock> FindEditableAsync(int id, CancellationToken cancellationToken)
    {
        var block = await _templateColumnBlockData.FindForEditAsync(id, cancellationToken);
        if (block is null)
        {
            throw new NotFoundException($"Column block {id} was not found.");
        }

        await _guard.EnsureEditableAsync(block.TableTemplateVersionId, cancellationToken);
        return block;
    }

    private Task<List<DbTemplateColumnBlock>> LoadSiblingsAsync(DbTemplateColumnBlock block, CancellationToken cancellationToken)
    {
        return _templateColumnBlockData.ListForEditAsync(block.TableTemplateVersionId, cancellationToken);
    }
}
