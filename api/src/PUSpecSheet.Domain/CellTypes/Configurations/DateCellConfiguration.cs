namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>Settings for a date cell.</summary>
public sealed record DateCellConfiguration : CellConfiguration
{
    protected override CellKind GetKind()
    {
        return CellKind.Date;
    }

    /// <summary>Whether a time of day is entered along with the date.</summary>
    public bool? IncludeTime { get; init; }

    public override CellConfiguration Apply(CellConfiguration? overrides)
    {
        if (overrides is not DateCellConfiguration date)
        {
            return this;
        }

        return new DateCellConfiguration { IncludeTime = date.IncludeTime ?? IncludeTime };
    }
}
