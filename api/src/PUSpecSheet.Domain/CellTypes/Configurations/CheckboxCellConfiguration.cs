namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>Settings for a checkbox cell. Checkboxes have none yet.</summary>
public sealed record CheckboxCellConfiguration : CellConfiguration
{
    protected override CellKind GetKind()
    {
        return CellKind.Checkbox;
    }

    public override CellConfiguration Apply(CellConfiguration? overrides)
    {
        return this;
    }
}
