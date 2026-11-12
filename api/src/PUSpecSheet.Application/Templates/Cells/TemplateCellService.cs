using PUSpecSheet.Application.CellTypes.Queries;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Templates.Queries;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Templates;

namespace PUSpecSheet.Application.Templates.Cells;

public sealed class TemplateCellService : ITemplateCellService
{
    private readonly PuSpecSheetDbContext db;
    private readonly TableTemplateReader reader;
    private readonly TemplateVersionGuard guard;

    public TemplateCellService(
        PuSpecSheetDbContext db,
        TableTemplateReader reader,
        TemplateVersionGuard guard)
    {
        this.db = db;
        this.reader = reader;
        this.guard = guard;
    }

    public async Task<TableTemplateDto> CreateAsync(CreateTemplateCellRequest request, CancellationToken cancellationToken)
    {
        var row = await LoadTargetAsync(request, cancellationToken);
        await guard.EnsureEditableAsync(row.TableTemplateVersionId, cancellationToken);

        if (request.TemplateColumnBlockId is int blockId)
        {
            await EnsureBlockInVersionAsync(blockId, row.TableTemplateVersionId, cancellationToken);
        }

        var cellTypeId = request.CellTypeId ?? await DefaultCellType.GetIdAsync(db, cancellationToken);
        await EnsureCellTypeExistsAsync(cellTypeId, cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var column = request.Column ?? row.NextColumn;
        if (request.Column is not null)
        {
            await MakeRoomAsync(request, column, cancellationToken);
        }

        db.TemplateCells.Add(new TemplateCell
        {
            TemplateRowId = request.TemplateRowId,
            TemplateColumnBlockId = request.TemplateColumnBlockId,
            Column = column,
            CellTypeId = cellTypeId,
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await reader.ReadVersionAsync(row.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> UpdateAsync(
        int id,
        UpdateTemplateCellRequest request,
        CancellationToken cancellationToken)
    {
        var cell = await FindEditableAsync(id, cancellationToken);
        await EnsureCellTypeExistsAsync(request.CellTypeId, cancellationToken);

        var columnTaken = await TemplateCellQueries.ColumnTakenAsync(db, cell.TemplateRowId, cell.TemplateColumnBlockId, request.Column, id, cancellationToken);

        if (columnTaken)
        {
            var place = cell.TemplateColumnBlockId is null ? "this row" : "this row's column block";
            throw new ConflictException($"Another cell in {place} already starts at column {request.Column}.");
        }

        cell.CellTypeId = request.CellTypeId;
        cell.Column = request.Column;
        cell.RowSpan = request.RowSpan;
        cell.ColumnSpan = request.ColumnSpan;
        cell.Caption = string.IsNullOrWhiteSpace(request.Caption) ? null : request.Caption.Trim();
        cell.IsRequired = request.IsRequired;

        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadVersionAsync(cell.TemplateRow.TemplateSection.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var cell = await FindEditableAsync(id, cancellationToken);

        db.TemplateCells.Remove(cell);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadVersionAsync(cell.TemplateRow.TemplateSection.TableTemplateVersionId, cancellationToken);
    }

    private async Task<TemplateCell> FindEditableAsync(int id, CancellationToken cancellationToken)
    {
        var cell = await TemplateCellQueries.FindForEditAsync(db, id, cancellationToken);

        if (cell is null)
        {
            throw new NotFoundException($"Cell {id} was not found.");
        }

        await guard.EnsureEditableAsync(cell.TemplateRow.TemplateSection.TableTemplateVersionId, cancellationToken);
        return cell;
    }

    private async Task EnsureBlockInVersionAsync(int blockId, int versionId, CancellationToken cancellationToken)
    {
        var exists = await TemplateColumnBlockQueries.IsInVersionAsync(db, blockId, versionId, cancellationToken);
        if (!exists)
        {
            throw new InvalidRequestException($"Column block {blockId} isn't part of this table version.");
        }
    }

    private async Task EnsureCellTypeExistsAsync(int cellTypeId, CancellationToken cancellationToken)
    {
        var exists = await CellTypeQueries.ExistsAsync(db, cellTypeId, cancellationToken);
        if (!exists)
        {
            throw new InvalidRequestException($"Cell type {cellTypeId} doesn't exist.");
        }
    }

    private async Task<TemplateCellTarget> LoadTargetAsync(CreateTemplateCellRequest request, CancellationToken cancellationToken)
    {
        return await TemplateRowQueries.FindCellTargetAsync(db, request.TemplateRowId, request.TemplateColumnBlockId, cancellationToken)
            ?? throw new NotFoundException($"Row {request.TemplateRowId} was not found.");
    }

    /// <summary>Moves the cells from this column on one to the right.</summary>
    private Task MakeRoomAsync(CreateTemplateCellRequest request, int column, CancellationToken cancellationToken)
    {
        return TemplateCellQueries.ShiftRightAsync(db, request.TemplateRowId, request.TemplateColumnBlockId, column, cancellationToken);
    }
}
