using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Finds, for the linked dropdown cells that also give their choice's row, which row of the source table
/// that is at the version. One query reads the cells' settings; a sheet with no such cell stops there.
/// Otherwise each column that is linked to is read once, with its values.
/// </summary>
public sealed class PublishedLinkedRowReader(
    PuSpecSheetDbContext db,
    PublishedLookupCellReader cellReader,
    PublishedKindValueReader valueReader)
{
    private static readonly Dictionary<int, Guid> None = [];

    /// <param name="version">The version to read.</param>
    /// <param name="structure">What was on the sheet at the version.</param>
    /// <param name="cells">The cells that may be linked, each with its row revision, or null for every cell of the sheet.</param>
    /// <param name="values">The values already read, by cell id. A linked cell with no value has no row.</param>
    /// <param name="cancellationToken">Stops the queries.</param>
    /// <returns>The source row's public identifier, by linked cell id.</returns>
    public async Task<IReadOnlyDictionary<int, Guid>> ReadAsync(
        ResolvedSheetVersion version,
        PublishedStructure structure,
        IReadOnlyCollection<(int RowRevisionId, int CellId)>? cells,
        IReadOnlyDictionary<int, object> values,
        CancellationToken cancellationToken)
    {
        if (cells is { Count: 0 })
        {
            return None;
        }

        var settings = await Settings(version, cells)
            .Select(value => new { value.SheetCellId, value.Settings })
            .ToListAsync(cancellationToken);
        var linked = settings
            .Select(entry => (entry.SheetCellId, Settings: entry.Settings as LinkedDropdownInstanceSettings))
            .Where(entry => entry.Settings is { ShowsRowValues: true } && values.ContainsKey(entry.SheetCellId))
            .ToList();
        if (linked.Count == 0)
        {
            return None;
        }

        var result = new Dictionary<int, Guid>();
        foreach (var source in linked.GroupBy(entry => (TableId: entry.Settings!.SourceSheetTableId!.Value, TemplateCellId: entry.Settings.SourceTemplateCellId!.Value)))
        {
            var column = await cellReader.QueryColumn(version, source.Key.TableId, source.Key.TemplateCellId).ToListAsync(cancellationToken);
            var columnValues = await valueReader.ReadAsync(version, column, wholeSheet: false, cancellationToken);
            var rows = PublishedLinkedRows.RowByOption(structure, column, columnValues);
            foreach (var cell in source)
            {
                if (rows.TryGetValue(PublishedLinkedRows.OptionText(values[cell.SheetCellId]), out var rowId))
                {
                    result[cell.SheetCellId] = rowId;
                }
            }
        }

        return result;
    }

    /// <summary>The cells that can be linked, which are the only ones whose settings need reading.</summary>
    public static List<(int RowRevisionId, int CellId)> LinkedCells(IEnumerable<PublishedLookupCellRecord> cells)
    {
        return cells
            .Where(cell => cell.Kind == CellKind.LinkedDropdown)
            .Select(cell => (cell.RowRevisionId, cell.CellId))
            .ToList();
    }

    /// <summary>The settings chosen on the sheet for the cells, in the row revisions the version shows.</summary>
    public IQueryable<CellSettingsValue> Settings(
        ResolvedSheetVersion version,
        IReadOnlyCollection<(int RowRevisionId, int CellId)>? cells)
    {
        return PublishedValueReader.Scope(
            db.CellSettings,
            version,
            cells?.Select(cell => cell.RowRevisionId).Distinct().ToList(),
            cells?.Select(cell => cell.CellId).ToList());
    }
}
