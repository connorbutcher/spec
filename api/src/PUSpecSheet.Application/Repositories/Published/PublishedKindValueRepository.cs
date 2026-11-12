using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Published;
using PUSpecSheet.Application.Published.Lookups;
using PUSpecSheet.Application.Published.Versions;
using PUSpecSheet.Domain.CellTypes;

using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Repositories.Published;

/// <summary>
/// Reads the values of cells whose kinds are known, and only asks the value tables those kinds use: rows
/// of text and numbers cost two queries, not one for every kind of value there is.
/// </summary>
public sealed class PublishedKindValueRepository : IPublishedKindValueRepository
{
    private readonly PuSpecSheetDbContext db;

    public PublishedKindValueRepository(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    /// <param name="cells">The cells to read values for.</param>
    /// <param name="wholeSheet">The version, when the cells are every cell of the sheet, so every value of its revisions is read and not a long list of cells; otherwise null.</param>
    /// <param name="cancellationToken">Stops the queries.</param>
    public async Task<Dictionary<int, object>> ReadAsync(
        IReadOnlyList<PublishedLookupCellRecord> cells,
        ResolvedSheetVersion? wholeSheet,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, object>();
        var valueCells = cells.Where(cell => cell.Kind.StoresValue()).ToList();
        if (valueCells.Count == 0)
        {
            return result;
        }

        var revisionIds = PublishedValueRepository.RevisionsOf(db, valueCells.Select(cell => cell.RowRevisionId), wholeSheet);
        if (CellsOf(valueCells, wholeSheet, out var textIds, CellKind.Text, CellKind.LinkedDropdown))
        {
            await PublishedValueRepository.ReadTextsAsync(db, result, revisionIds, textIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var numberIds, CellKind.Number))
        {
            await PublishedValueRepository.ReadNumbersAsync(db, result, revisionIds, numberIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var dateIds, CellKind.Date))
        {
            await PublishedValueRepository.ReadDatesAsync(db, result, revisionIds, dateIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var booleanIds, CellKind.Checkbox))
        {
            await PublishedValueRepository.ReadBooleansAsync(db, result, revisionIds, booleanIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var optionIds, CellKind.TextDropdown, CellKind.NumberDropdown))
        {
            await PublishedValueRepository.ReadOptionsAsync(db, result, revisionIds, optionIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var labelledIds, CellKind.LabelledText))
        {
            await PublishedValueRepository.ReadLabelledTextsAsync(db, result, revisionIds, labelledIds, cancellationToken);
        }

        return result;
    }

    /// <summary>Whether any cell is of the kinds, and the ids of those cells (null for a whole sheet).</summary>
    private static bool CellsOf(List<PublishedLookupCellRecord> cells, ResolvedSheetVersion? wholeSheet, out List<int>? ids, params CellKind[] kinds)
    {
        var matching = cells.Where(cell => kinds.Contains(cell.Kind)).Select(cell => cell.CellId).ToList();
        ids = wholeSheet is not null ? null : matching;
        return matching.Count > 0;
    }
}
