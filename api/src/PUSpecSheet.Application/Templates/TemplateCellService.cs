using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

public sealed class TemplateCellService(PuSpecSheetDbContext db, TableTemplateReader reader) : ITemplateCellService
{
    public async Task<TableTemplateDto> CreateAsync(CreateTemplateCellRequest request, CancellationToken cancellationToken)
    {
        var row = await db.TemplateRows
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.TemplateRowId)
            .Select(candidate => new
            {
                candidate.TemplateSection.TableTemplateId,
                NextColumn = candidate.Cells.Max(cell => (int?)(cell.Column + cell.ColumnSpan)) ?? 1,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new NotFoundException($"Row {request.TemplateRowId} was not found.");
        }

        var cellTypeId = request.CellTypeId ?? await DefaultCellType.GetIdAsync(db, cancellationToken);
        await EnsureCellTypeExistsAsync(cellTypeId, cancellationToken);

        db.TemplateCells.Add(new TemplateCell
        {
            TemplateRowId = request.TemplateRowId,
            Column = row.NextColumn,
            CellTypeId = cellTypeId,
        });

        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(row.TableTemplateId, cancellationToken);
    }

    public async Task<TableTemplateDto> UpdateAsync(
        int id,
        UpdateTemplateCellRequest request,
        CancellationToken cancellationToken)
    {
        var cell = await FindAsync(id, cancellationToken);
        await EnsureCellTypeExistsAsync(request.CellTypeId, cancellationToken);

        var columnTaken = await db.TemplateCells.AnyAsync(
            other => other.TemplateRowId == cell.TemplateRowId && other.Column == request.Column && other.Id != id,
            cancellationToken);

        if (columnTaken)
        {
            throw new ConflictException($"Another cell in this row already starts at column {request.Column}.");
        }

        cell.CellTypeId = request.CellTypeId;
        cell.Column = request.Column;
        cell.RowSpan = request.RowSpan;
        cell.ColumnSpan = request.ColumnSpan;
        cell.Caption = string.IsNullOrWhiteSpace(request.Caption) ? null : request.Caption.Trim();
        cell.IsRequired = request.IsRequired;

        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(cell.TemplateRow.TemplateSection.TableTemplateId, cancellationToken);
    }

    public async Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var cell = await FindAsync(id, cancellationToken);
        await SheetDataGuard.EnsureCellsUnusedAsync(db, [id], "this cell", cancellationToken);

        db.TemplateCells.Remove(cell);
        await db.SaveChangesAsync(cancellationToken);

        return await reader.ReadAsync(cell.TemplateRow.TemplateSection.TableTemplateId, cancellationToken);
    }

    private async Task<TemplateCell> FindAsync(int id, CancellationToken cancellationToken)
    {
        var cell = await db.TemplateCells
            .Include(candidate => candidate.TemplateRow)
            .ThenInclude(row => row.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (cell is null)
        {
            throw new NotFoundException($"Cell {id} was not found.");
        }

        return cell;
    }

    private async Task EnsureCellTypeExistsAsync(int cellTypeId, CancellationToken cancellationToken)
    {
        var exists = await db.CellTypes.AnyAsync(cellType => cellType.Id == cellTypeId, cancellationToken);
        if (!exists)
        {
            throw new InvalidRequestException($"Cell type {cellTypeId} doesn't exist.");
        }
    }
}
