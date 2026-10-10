using System.Text.Json.Serialization;

namespace PUSpecSheet.Domain.CellTypes.InstanceSettings;

/// <summary>
/// Where a linked dropdown cell takes its choices from: one column of another table on the same sheet.
/// The choices are the values that column holds in every row of that table.
/// </summary>
public sealed record LinkedDropdownInstanceSettings : CellInstanceSettings
{
    /// <summary>The sheet table the choices come from.</summary>
    public int? SourceSheetTableId { get; init; }

    /// <summary>The column of that table: the template cell its cells were built from.</summary>
    public int? SourceTemplateCellId { get; init; }

    /// <summary>
    /// True when the cell also gives the rest of the row its choice was picked from: every other value in
    /// that row of the source table. They are read from the source row each time, never stored with this
    /// cell. Null, not false, when off.
    /// </summary>
    public bool? IncludeRowValues { get; init; }

    /// <summary>Whether a choice also brings the other values of the row it came from.</summary>
    [JsonIgnore]
    public bool ShowsRowValues => IncludeRowValues == true && SourceSheetTableId is not null && SourceTemplateCellId is not null;

    public override CellInstanceSettings Normalise()
    {
        return IncludeRowValues == false ? this with { IncludeRowValues = null } : this;
    }

    /// <summary>The choices are the same while the cell reads the same column, whatever else is switched on.</summary>
    public override bool OffersSameChoicesAs(CellInstanceSettings? other)
    {
        return other is LinkedDropdownInstanceSettings linked
            && linked.SourceSheetTableId == SourceSheetTableId
            && linked.SourceTemplateCellId == SourceTemplateCellId;
    }

    protected override CellKind GetKind()
    {
        return CellKind.LinkedDropdown;
    }
}
