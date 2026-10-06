namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>Settings for a number cell.</summary>
public sealed record NumberCellConfiguration : CellConfiguration
{
    protected override CellKind GetKind()
    {
        return CellKind.Number;
    }

    /// <summary>How many decimal places a value is shown and entered with.</summary>
    public int? DecimalPlaces { get; init; }

    /// <summary>The smallest allowed value.</summary>
    public decimal? MinValue { get; init; }

    /// <summary>The largest allowed value.</summary>
    public decimal? MaxValue { get; init; }

    /// <summary>The unit shown after the value, e.g. "Nm".</summary>
    public string? Unit { get; init; }

    public override CellConfiguration Apply(CellConfiguration? cellOverride)
    {
        if (cellOverride is not NumberCellConfiguration number)
        {
            return this;
        }

        return new NumberCellConfiguration
        {
            DecimalPlaces = number.DecimalPlaces ?? DecimalPlaces,
            MinValue = number.MinValue ?? MinValue,
            MaxValue = number.MaxValue ?? MaxValue,
            Unit = number.Unit ?? Unit,
        };
    }

    public override string? Validate()
    {
        if (MinValue > MaxValue)
        {
            return "The minimum value can't be more than the maximum value.";
        }

        return NumberRules.CheckDecimalPlaces(DecimalPlaces) ?? NumberRules.CheckUnit(Unit);
    }
}
