using PUSpecSheet.Abstractions.CellTypes.Configurations;

namespace PUSpecSheet.DataAccess.CellTypes;

internal static class CellSettingText
{
    public static string? NullIfBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static CellConfiguration Trim(CellConfiguration configuration)
    {
        return configuration switch
        {
            NumberCellConfiguration number => number with { Unit = NullIfBlank(number.Unit) },
            NumberDropdownCellConfiguration dropdown => dropdown with { Unit = NullIfBlank(dropdown.Unit) },
            _ => configuration,
        };
    }
}
