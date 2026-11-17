using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.CellTypes;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Templates.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates.Cells;

public sealed class TemplateCellsService : ITemplateCellsService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly TableTemplateReader _reader;
    private readonly TemplateVersionGuard _guard;
    private readonly ICellTypeDataService _cellTypes;

    public TemplateCellsService(
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

    public async Task<TableTemplateDto> CreateAsync(CreateTemplateCellRequest request, CancellationToken cancellationToken)
    {
        var row = await LoadTargetAsync(request, cancellationToken);
        await _guard.EnsureEditableAsync(row.TableTemplateVersionId, cancellationToken);

        if (request.TemplateColumnBlockId is int blockId)
        {
            await EnsureBlockInVersionAsync(blockId, row.TableTemplateVersionId, cancellationToken);
        }

        var cellTypeId = request.CellTypeId ?? await _cellTypes.GetDefaultIdAsync(cancellationToken);
        await EnsureCellTypeExistsAsync(cellTypeId, cancellationToken);

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var column = request.Column ?? row.NextColumn;
        if (request.Column is not null)
        {
            await MakeRoomAsync(request, column, cancellationToken);
        }

        _db.TemplateCells.Add(new DbTemplateCell
        {
            TemplateRowId = request.TemplateRowId,
            TemplateColumnBlockId = request.TemplateColumnBlockId,
            Column = column,
            CellTypeId = cellTypeId,
        });

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await _reader.ReadVersionAsync(row.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> UpdateAsync(
        int id,
        UpdateTemplateCellRequest request,
        CancellationToken cancellationToken)
    {
        var cell = await FindEditableAsync(id, cancellationToken);
        await EnsureCellTypeExistsAsync(request.CellTypeId, cancellationToken);

        var columnTaken = await TemplateCellQueries.ColumnTakenAsync(_db, cell.TemplateRowId, cell.TemplateColumnBlockId, request.Column, id, cancellationToken);

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

        await _db.SaveChangesAsync(cancellationToken);
        return await _reader.ReadVersionAsync(cell.TemplateRow.TemplateSection.TableTemplateVersionId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var cell = await FindEditableAsync(id, cancellationToken);

        _db.TemplateCells.Remove(cell);
        await _db.SaveChangesAsync(cancellationToken);

        return await _reader.ReadVersionAsync(cell.TemplateRow.TemplateSection.TableTemplateVersionId, cancellationToken);
    }

    private async Task<DbTemplateCell> FindEditableAsync(int id, CancellationToken cancellationToken)
    {
        var cell = await TemplateCellQueries.FindForEditAsync(_db, id, cancellationToken);

        if (cell is null)
        {
            throw new NotFoundException($"Cell {id} was not found.");
        }

        await _guard.EnsureEditableAsync(cell.TemplateRow.TemplateSection.TableTemplateVersionId, cancellationToken);
        return cell;
    }

    private async Task EnsureBlockInVersionAsync(int blockId, int versionId, CancellationToken cancellationToken)
    {
        var exists = await TemplateColumnBlockQueries.IsInVersionAsync(_db, blockId, versionId, cancellationToken);
        if (!exists)
        {
            throw new InvalidRequestException($"Column block {blockId} isn't part of this table version.");
        }
    }

    private async Task EnsureCellTypeExistsAsync(int cellTypeId, CancellationToken cancellationToken)
    {
        var exists = await _cellTypes.ExistsAsync(cellTypeId, cancellationToken);
        if (!exists)
        {
            throw new InvalidRequestException($"Cell type {cellTypeId} doesn't exist.");
        }
    }

    private async Task<TemplateCellTarget> LoadTargetAsync(CreateTemplateCellRequest request, CancellationToken cancellationToken)
    {
        return await TemplateRowQueries.FindCellTargetAsync(_db, request.TemplateRowId, request.TemplateColumnBlockId, cancellationToken)
            ?? throw new NotFoundException($"Row {request.TemplateRowId} was not found.");
    }

    /// <summary>Moves the cells from this column on one to the right.</summary>
    private Task MakeRoomAsync(CreateTemplateCellRequest request, int column, CancellationToken cancellationToken)
    {
        return TemplateCellQueries.ShiftRightAsync(_db, request.TemplateRowId, request.TemplateColumnBlockId, column, cancellationToken);
    }
}
