using PUSpecSheet.Domain.CellTypes.Configurations;

namespace PUSpecSheet.Application.CellTypes;

/// <summary>Tidies the free text in cell settings, so blank text means "not set".</summary>
internal static class CellSettingText
{
    public static string? NullIfBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>The configuration with its units trimmed, and blank units treated as not set.</summary>
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
