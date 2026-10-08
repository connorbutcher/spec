namespace PUSpecSheet.Domain.CellTypes.Configurations;

public static class CellConfigurations
{
    /// <summary>A configuration for <paramref name="kind"/> with nothing set.</summary>
    public static CellConfiguration CreateEmpty(CellKind kind)
    {
        return kind switch
        {
            CellKind.Heading => new HeadingCellConfiguration(),
            CellKind.Group => new GroupCellConfiguration(),
            CellKind.Text => new TextCellConfiguration(),
            CellKind.Number => new NumberCellConfiguration(),
            CellKind.Date => new DateCellConfiguration(),
            CellKind.Checkbox => new CheckboxCellConfiguration(),
            CellKind.TextDropdown => new TextDropdownCellConfiguration(),
            CellKind.NumberDropdown => new NumberDropdownCellConfiguration(),
            CellKind.LinkedDropdown => new LinkedDropdownCellConfiguration(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown cell kind."),
        };
    }

    /// <summary>Whether <paramref name="configuration"/> sets nothing, so as an override it changes nothing.</summary>
    public static bool IsEmpty(this CellConfiguration configuration)
    {
        return configuration == CreateEmpty(configuration.Kind);
    }
}
