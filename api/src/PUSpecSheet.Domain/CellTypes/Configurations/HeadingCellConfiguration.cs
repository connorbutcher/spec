namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>Settings for a heading cell. Headings have none yet.</summary>
public sealed record HeadingCellConfiguration : CellConfiguration
{
    protected override CellKind GetKind()
    {
        return CellKind.Heading;
    }

    public override CellConfiguration Apply(CellConfiguration? overrides)
    {
        return this;
    }
}
