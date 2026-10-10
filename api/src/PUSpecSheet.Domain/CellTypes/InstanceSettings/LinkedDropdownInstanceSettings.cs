using System.Text.Json.Serialization;

namespace PUSpecSheet.Domain.CellTypes.InstanceSettings;

/// <summary>
/// Where a linked dropdown cell takes its choices from, in another table on the same sheet: either one
/// column of it, giving the values that column holds in every row, or one row of it, giving every value
/// in that row. Exactly one of <see cref="SourceTemplateCellId"/> and <see cref="SourceSheetRowId"/> is set.
/// </summary>
public sealed record LinkedDropdownInstanceSettings : CellInstanceSettings
{
    /// <summary>The sheet table the choices come from.</summary>
    public int? SourceSheetTableId { get; init; }

    /// <summary>For a column: the template cell that column's cells were built from.</summary>
    public int? SourceTemplateCellId { get; init; }

    /// <summary>For a row: the row of the table whose values are the choices.</summary>
    public int? SourceSheetRowId { get; init; }

    /// <summary>Whether a table and one of a column or a row are chosen, so there is something to read.</summary>
    [JsonIgnore]
    public bool HasSource => SourceSheetTableId is not null && (SourceTemplateCellId is null) != (SourceSheetRowId is null);

    protected override CellKind GetKind()
    {
        return CellKind.LinkedDropdown;
    }
}
