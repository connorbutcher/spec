namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>
/// Settings for a number dropdown cell. Its choices are the cell type's options, each of which must be
/// a number; a value chosen on a sheet is stored as the option.
/// </summary>
public sealed record NumberDropdownCellConfiguration : CellConfiguration
{
    protected override CellKind GetKind()
    {
        return CellKind.NumberDropdown;
    }

    /// <summary>How many decimal places the options are shown with.</summary>
    public int? DecimalPlaces { get; init; }

    /// <summary>The unit shown after the value, e.g. "mm".</summary>
    public string? Unit { get; init; }

    public override CellConfiguration Apply(CellConfiguration? cellOverride)
    {
        if (cellOverride is not NumberDropdownCellConfiguration number)
        {
            return this;
        }

        return new NumberDropdownCellConfiguration
        {
            DecimalPlaces = number.DecimalPlaces ?? DecimalPlaces,
            Unit = number.Unit ?? Unit,
        };
    }

    public override string? Validate()
    {
        return NumberRules.CheckDecimalPlaces(DecimalPlaces) ?? NumberRules.CheckUnit(Unit);
    }
}
