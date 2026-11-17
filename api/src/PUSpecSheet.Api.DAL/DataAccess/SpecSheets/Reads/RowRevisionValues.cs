using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

/// <summary>
/// The values a row revision holds, by the cell they are for, given to that row's cells as the value rows a copy has.
/// </summary>
internal class RowRevisionValues
{
    private readonly Dictionary<int, DbSheetTextValue> _texts;
    private readonly Dictionary<int, DbSheetNumericValue> _numbers;
    private readonly Dictionary<int, DbSheetPairValue> _pairs;
    private readonly Dictionary<int, DbSheetLinkedSheetValue> _links;
    private readonly Dictionary<int, DbSheetCellConfigurationOverride> _overrides;

    public RowRevisionValues(DbSheetTableRowRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision, nameof(revision));

        _texts = revision.TextValues.ToDictionary(value => value.SheetTableCellId);
        _numbers = revision.NumericValues.ToDictionary(value => value.SheetTableCellId);
        _pairs = revision.PairValues.ToDictionary(value => value.SheetTableCellId);
        _links = revision.LinkedSheetValues.ToDictionary(value => value.SheetTableCellId);
        _overrides = revision.CellConfigurationOverrides.ToDictionary(value => value.SheetTableCellId);
    }

    public ICellProperties? ConfigurationOverrideOf(int cellId)
    {
        return _overrides.GetValueOrDefault(cellId)?.ConfigurationOverride;
    }

    /// <summary>
    /// Gives the cell its value. A copied cell always has a value row; where the revision holds no value, it is an
    /// empty one of the kind the cell's type holds, as the app gives a new cell.
    /// </summary>
    public void SetValue(DbSpecSheetTableCell cell, string modelType)
    {
        if (_numbers.TryGetValue(cell.Id, out var number))
        {
            cell.NumericValue = new DbNumericValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, Value = number.Value };
        }
        else if (_texts.TryGetValue(cell.Id, out var text))
        {
            cell.TextValue = new DbTextValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, Value = text.Value };
        }
        else if (_pairs.TryGetValue(cell.Id, out var pair))
        {
            cell.PairValue = new DbPairValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, PairValueX = pair.PairValueX, PairValueY = pair.PairValueY };
        }
        else if (_links.TryGetValue(cell.Id, out var link))
        {
            cell.LinkedSpecSheetValue = LinkedValue(cell.Id, link.TableReferenceId, link.LinkedSpecSheetTypeId, link.LinkedSpecSheetPhaseId, link.LinkedReferenceId);
        }
        else
        {
            SetEmptyValue(cell, modelType);
        }
    }

    private static void SetEmptyValue(DbSpecSheetTableCell cell, string modelType)
    {
        switch (modelType)
        {
            case "numeric":
            case "numericDropdown":
                cell.NumericValue = new DbNumericValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, Value = null };
                break;
            case "pair":
                cell.PairValue = new DbPairValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, PairValueX = null, PairValueY = null };
                break;
            case "linkedSpecSheet":
                cell.LinkedSpecSheetValue = LinkedValue(cell.Id, null, null, null, null);
                break;
            default:
                cell.TextValue = new DbTextValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, Value = null };
                break;
        }
    }

    private static DbLinkedSpecSheetValue LinkedValue(int cellId, Guid? tableReferenceId, int? typeId, int? phaseId, Guid? linkedReferenceId)
    {
        return new DbLinkedSpecSheetValue
        {
            Id = cellId,
            SpecSheetTableCellId = cellId,
            TableReferenceId = tableReferenceId,
            LinkedSpecSheetTypeId = typeId,
            LinkedSpecSheetPhaseId = phaseId,
            LinkedReferenceId = linkedReferenceId
        };
    }
}
