namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>
/// Settings for a linked dropdown cell. Its choices aren't set here: each cell is pointed at a column of
/// another table on its sheet (see <see cref="InstanceSettings.LinkedDropdownInstanceSettings"/>).
/// </summary>
public sealed record LinkedDropdownCellConfiguration : CellConfiguration
{
    protected override CellKind GetKind()
    {
        return CellKind.LinkedDropdown;
    }

    public override CellConfiguration Apply(CellConfiguration? cellOverride)
    {
        return this;
    }
}
