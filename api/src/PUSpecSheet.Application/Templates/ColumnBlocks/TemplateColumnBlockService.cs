using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.CellTypes;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Application.Templates.Sections;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Database.Entities.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates.ColumnBlocks;

public sealed class TemplateColumnBlockService : ITemplateColumnBlockService
{
    private readonly ITableTemplateRepository templates;
    private readonly ITemplateRowRepository rows;
    private readonly ITemplateCellRepository cells;
    private readonly ITemplateColumnBlockRepository columnBlocks;
    private readonly ICellTypeRepository cellTypes;
    private readonly IUnitOfWork unitOfWork;
    private readonly TableTemplateReader reader;
    private readonly TemplateVersionGuard guard;

    public TemplateColumnBlockService(
        ITableTemplateRepository templates,
        ITemplateRowRepository rows,
        ITemplateCellRepository cells,
        ITemplateColumnBlockRepository columnBlocks,
        ICellTypeRepository cellTypes,
        IUnitOfWork unitOfWork,
        TableTemplateReader reader,
        TemplateVersionGuard guard)
    {
        this.templates = templates;
        this.rows = rows;
        this.cells = cells;
        this.columnBlocks = columnBlocks;
        this.cellTypes = cellTypes;
        this.unitOfWork = unitOfWork;
        this.reader = reader;
        this.guard = guard;
    }

    public async Task<TableTemplateDto> CreateAsync(
        CreateTemplateColumnBlockRequest request,
        CancellationToken cancellationToken)
    {
        var versionId = request.TableTemplateVersionId;
        await guard.EnsureEditableAsync(versionId, cancellationToken);
        await EnsureHorizontalAsync(versionId, cancellationToken);

        var lastOrder = await columnBlocks.GetLastDisplayOrderAsync(versionId, cancellationToken);

        var rowIds = await rows.ListIdsAsync(versionId, cancellationToken);

        var cellTypeId = await DefaultCellType.GetIdAsync(cellTypes, cancellationToken);

        // A block shows in every row, so each row starts with one cell in it.
        var block = new TemplateColumnBlock
        {
            TableTemplateVersionId = versionId,
            Name = request.Name.Trim(),
            DisplayOrder = (lastOrder ?? 0) + 1,
            MinInstances = 0,
            MaxInstances = null,
            InitialInstances = 1,
            Cells = rowIds
                .Select(rowId => new TemplateCell { TemplateRowId = rowId, Column = 1, CellTypeId = cellTypeId })
                .ToList(),
        };

        columnBlocks.Add(block);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await reader.ReadVersionAsync(versionId, cancellationToken);
    }

    public async Task<TableTemplateDto> UpdateAsync(
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

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await reader.ReadVersionAsync(block.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var block = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(block, cancellationToken);

        DisplayOrdering.Move(siblings, block, request.DisplayOrder, sibling => sibling.DisplayOrder, SetOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(block.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var block = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(block, cancellationToken);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        // The block's cells can't cascade from it (they already cascade from their rows), so they go first.
        await cells.DeleteInColumnBlockAsync(id, cancellationToken);

        columnBlocks.Remove(block);
        DisplayOrdering.Renumber(siblings.Where(sibling => sibling.Id != id), sibling => sibling.DisplayOrder, SetOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await reader.ReadVersionAsync(block.TableTemplateVersionId, cancellationToken);
    }

    private static void SetOrder(TemplateColumnBlock block, int order)
    {
        block.DisplayOrder = order;
    }

    private async Task EnsureHorizontalAsync(int versionId, CancellationToken cancellationToken)
    {
        var orientation = await templates.GetOrientationAsync(versionId, cancellationToken);

        if (orientation != TemplateOrientation.Horizontal)
        {
            throw new ConflictException("Only horizontal tables have column blocks. Make the table horizontal first.");
        }
    }

    private async Task<TemplateColumnBlock> FindEditableAsync(int id, CancellationToken cancellationToken)
    {
        var block = await columnBlocks.FindForEditAsync(id, cancellationToken);
        if (block is null)
        {
            throw new NotFoundException($"Column block {id} was not found.");
        }

        await guard.EnsureEditableAsync(block.TableTemplateVersionId, cancellationToken);
        return block;
    }

    private Task<List<TemplateColumnBlock>> LoadSiblingsAsync(TemplateColumnBlock block, CancellationToken cancellationToken)
    {
        return columnBlocks.ListForEditAsync(block.TableTemplateVersionId, cancellationToken);
    }
}
