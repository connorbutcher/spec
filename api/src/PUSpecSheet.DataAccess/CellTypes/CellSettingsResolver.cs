using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.CellTypes;

public static class CellSettingsResolver
{
    // Expects cell.CellType to be loaded.
    public static CellConfiguration ResolveConfiguration(DbTemplateCell cell)
    {
        return cell.CellType.Configuration.Apply(cell.ConfigurationOverride);
    }
}
