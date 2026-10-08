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

    protected override CellKind GetKind()
    {
        return CellKind.LinkedDropdown;
    }
}
