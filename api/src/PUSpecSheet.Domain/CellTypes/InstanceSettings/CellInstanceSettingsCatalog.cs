namespace PUSpecSheet.Domain.CellTypes.InstanceSettings;

/// <summary>Which cell kinds have settings chosen on the sheet, and what those settings start as.</summary>
public static class CellInstanceSettingsCatalog
{
    /// <summary>Settings for <paramref name="kind"/> with nothing set, or null for a kind that has none.</summary>
    public static CellInstanceSettings? CreateEmpty(CellKind kind)
    {
        return kind switch
        {
            CellKind.LinkedDropdown => new LinkedDropdownInstanceSettings(),
            _ => null,
        };
    }

    /// <summary>Whether cells of this kind have settings chosen on the sheet.</summary>
    public static bool HasInstanceSettings(this CellKind kind)
    {
        return CreateEmpty(kind) is not null;
    }
}
