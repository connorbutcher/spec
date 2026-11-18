using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// Reads the values of a sheet's copied cells, by cell. A cell has at most one value of each kind.
/// </summary>
internal sealed class LegacyCellValueReader
{
    private readonly PUSpecSheetDbContext _dbContext;

    public LegacyCellValueReader(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    // A cell has at most one value of each kind; where a copy has more, the newest (highest id) is kept.
    public async Task<Dictionary<int, CellContentParts>> ReadAsync(
        IQueryable<DbSpecSheetTableCell> cells,
        List<string> warnings,
        CancellationToken cancellationToken)
    {
        var parts = new Dictionary<int, CellContentParts>();
        var repeated = 0;

        foreach (var value in await _dbContext.TextValues.AsNoTracking()
            .Where(value => value.Value != null && cells.Any(cell => cell.Id == value.SpecSheetTableCellId))
            .OrderBy(value => value.Id)
            .Select(value => new { value.SpecSheetTableCellId, value.Value })
            .ToListAsync(cancellationToken))
        {
            var cell = parts.GetOrNew(value.SpecSheetTableCellId);
            repeated += cell.Text is null ? 0 : 1;
            cell.Text = value.Value;
        }

        foreach (var value in await _dbContext.NumericValues.AsNoTracking()
            .Where(value => value.Value != null && cells.Any(cell => cell.Id == value.SpecSheetTableCellId))
            .OrderBy(value => value.Id)
            .Select(value => new { value.SpecSheetTableCellId, value.Value })
            .ToListAsync(cancellationToken))
        {
            var cell = parts.GetOrNew(value.SpecSheetTableCellId);
            repeated += cell.Number is null ? 0 : 1;
            cell.Number = value.Value;
        }

        foreach (var value in await _dbContext.PairValues.AsNoTracking()
            .Where(value => (value.PairValueX != null || value.PairValueY != null) && cells.Any(cell => cell.Id == value.SpecSheetTableCellId))
            .OrderBy(value => value.Id)
            .Select(value => new { value.SpecSheetTableCellId, value.PairValueX, value.PairValueY })
            .ToListAsync(cancellationToken))
        {
            var cell = parts.GetOrNew(value.SpecSheetTableCellId);
            repeated += cell.PairX is null && cell.PairY is null ? 0 : 1;
            cell.PairX = value.PairValueX;
            cell.PairY = value.PairValueY;
        }

        foreach (var value in await _dbContext.LinkedSpecSheetValues.AsNoTracking()
            .Where(value => cells.Any(cell => cell.Id == value.SpecSheetTableCellId))
            .OrderBy(value => value.Id)
            .Select(value => new
            {
                value.SpecSheetTableCellId,
                value.LinkedSpecSheetTypeId,
                value.LinkedSpecSheetPhaseId,
                value.TableReferenceId,
                value.LinkedReferenceId
            })
            .ToListAsync(cancellationToken))
        {
            var cell = parts.GetOrNew(value.SpecSheetTableCellId);
            repeated += cell.Linked is null ? 0 : 1;
            cell.Linked = new LinkedSheetTarget
            {
                SheetTypeId = value.LinkedSpecSheetTypeId,
                PhaseId = value.LinkedSpecSheetPhaseId,
                TableReferenceId = value.TableReferenceId,
                ItemReferenceId = value.LinkedReferenceId
            };
        }

        if (repeated > 0)
        {
            warnings.Add($"{repeated} value(s) were one of several of the same kind on one cell; the newest of each was kept.");
        }

        return parts;
    }
}
