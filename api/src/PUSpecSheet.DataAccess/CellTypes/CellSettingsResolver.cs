using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.CellTypes;

/// <summary>
/// Works out the settings a template cell actually uses: its cell type's defaults with the cell's
/// overrides laid on top.
/// </summary>
public static class CellSettingsResolver
{
    /// <summary>Expects <see cref="DbTemplateCell.CellType"/> to be loaded.</summary>
    public static CellConfiguration ResolveConfiguration(DbTemplateCell cell)
    {
        return cell.CellType.Configuration.Apply(cell.ConfigurationOverride);
    }
}
