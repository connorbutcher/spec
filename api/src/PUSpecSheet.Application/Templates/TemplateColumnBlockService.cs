using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

public sealed class TemplateColumnBlockService(
    PuSpecSheetDbContext db,
    TableTemplateReader reader,
    TemplateVersionGuard guard) : ITemplateColumnBlockService
{
    public async Task<TableTemplateDto> CreateAsync(
        CreateTemplateColumnBlockRequest request,
        CancellationToken cancellationToken)
    {
        var versionId = request.TableTemplateVersionId;
        await guard.EnsureEditableAsync(versionId, cancellationToken);
        await EnsureHorizontalAsync(versionId, cancellationToken);

        var lastOrder = await db.TemplateColumnBlocks
            .Where(block => block.TableTemplateVersionId == versionId)
            .MaxAsync(block => (int?)block.DisplayOrder, cancellationToken);

        var rowIds = await db.TemplateRows
            .Where(row => row.TemplateSection.TableTemplateVersionId == versionId)
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);

        var cellTypeId = await DefaultCellType.GetIdAsync(db, cancellationToken);

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

        db.TemplateColumnBlocks.Add(block);
        await db.SaveChangesAsync(cancellationToken);
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

        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadVersionAsync(block.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var block = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(block, cancellationToken);

        DisplayOrdering.Move(siblings, block, request.DisplayOrder, sibling => sibling.DisplayOrder, SetOrder);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(block.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var block = await FindEditableAsync(id, cancellationToken);
        var siblings = await LoadSiblingsAsync(block, cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // The block's cells can't cascade from it (they already cascade from their rows), so they go first.
        await db.TemplateCells
            .Where(cell => cell.TemplateColumnBlockId == id)
            .ExecuteDeleteAsync(cancellationToken);

        db.TemplateColumnBlocks.Remove(block);
        DisplayOrdering.Renumber(siblings.Where(sibling => sibling.Id != id), sibling => sibling.DisplayOrder, SetOrder);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await reader.ReadVersionAsync(block.TableTemplateVersionId, cancellationToken);
    }

    private static void SetOrder(TemplateColumnBlock block, int order)
    {
        block.DisplayOrder = order;
    }

    private async Task EnsureHorizontalAsync(int versionId, CancellationToken cancellationToken)
    {
        var orientation = await db.TableTemplateVersions
            .Where(version => version.Id == versionId)
            .Select(version => version.Orientation)
            .SingleAsync(cancellationToken);

        if (orientation != TemplateOrientation.Horizontal)
        {
            throw new ConflictException("Only horizontal tables have column blocks. Make the table horizontal first.");
        }
    }

    private async Task<TemplateColumnBlock> FindEditableAsync(int id, CancellationToken cancellationToken)
    {
        var block = await db.TemplateColumnBlocks.FindAsync([id], cancellationToken);
        if (block is null)
        {
            throw new NotFoundException($"Column block {id} was not found.");
        }

        await guard.EnsureEditableAsync(block.TableTemplateVersionId, cancellationToken);
        return block;
    }

    private Task<List<TemplateColumnBlock>> LoadSiblingsAsync(TemplateColumnBlock block, CancellationToken cancellationToken)
    {
        return db.TemplateColumnBlocks
            .Where(candidate => candidate.TableTemplateVersionId == block.TableTemplateVersionId)
            .ToListAsync(cancellationToken);
    }
}
