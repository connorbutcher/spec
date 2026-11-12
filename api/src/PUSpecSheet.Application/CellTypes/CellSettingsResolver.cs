using PUSpecSheet.Database.Entities.Templates;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Application.CellTypes;

/// <summary>
/// Works out the settings a template cell actually uses: its cell type's defaults with the cell's
/// overrides laid on top.
/// </summary>
public static class CellSettingsResolver
{
    /// <summary>Expects <see cref="TemplateCell.CellType"/> to be loaded.</summary>
    public static CellConfiguration ResolveConfiguration(TemplateCell cell)
    {
        return cell.CellType.Configuration.Apply(cell.ConfigurationOverride);
    }

    /// <summary>Expects <see cref="TemplateCell.CellType"/> to be loaded.</summary>
    public static CellStyle ResolveStyle(TemplateCell cell)
    {
        return cell.CellType.Style.Apply(cell.StyleOverride);
    }
}
