namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>Settings for a text dropdown cell. Its choices are the cell type options.</summary>
public sealed record TextDropdownCellConfiguration : CellConfiguration
{
    protected override CellKind GetKind()
    {
        return CellKind.TextDropdown;
    }

    public override CellConfiguration Apply(CellConfiguration? overrides)
    {
        return this;
    }
}
