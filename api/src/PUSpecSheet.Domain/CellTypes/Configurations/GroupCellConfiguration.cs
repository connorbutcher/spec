namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>Settings for a group cell. Groups have none yet.</summary>
public sealed record GroupCellConfiguration : CellConfiguration
{
    protected override CellKind GetKind()
    {
        return CellKind.Group;
    }

    public override CellConfiguration Apply(CellConfiguration? overrides)
    {
        return this;
    }
}
