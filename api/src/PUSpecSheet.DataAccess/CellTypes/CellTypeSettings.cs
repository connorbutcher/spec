using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.Styles;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database.Models.CellTypes;

namespace PUSpecSheet.DataAccess.CellTypes;

internal static class CellTypeSettings
{
    public static void Apply(SaveCellTypeRequest request, DbCellType cellType)
    {
        cellType.Name = request.Name.Trim();
        cellType.Kind = request.Kind;
        cellType.Description = CellSettingText.NullIfBlank(request.Description);
        cellType.Configuration = CleanConfiguration(request.Kind, request.Configuration);
        cellType.Style = CleanStyle(request.Style);

        var options = request.Kind.IsDropdown() ? CellTypeOptions.Clean(request.Kind, request.Options) : [];
        CellTypeOptions.Sync(cellType, options);
    }

    // A configuration for another kind is dropped, so a kind change starts with nothing set.
    public static CellConfiguration CleanConfiguration(CellKind kind, CellConfiguration? configuration)
    {
        if (configuration is null || configuration.Kind != kind)
        {
            return CellConfigurations.CreateEmpty(kind);
        }

        var cleaned = CellSettingText.Trim(configuration);
        var problem = cleaned.Validate();
        if (problem is not null)
        {
            throw new InvalidRequestException(problem);
        }

        return cleaned;
    }

    public static CellStyle CleanStyle(CellStyle? style)
    {
        var cleaned = style ?? new CellStyle();
        var problem = cleaned.Validate();
        if (problem is not null)
        {
            throw new InvalidRequestException(problem);
        }

        return cleaned with
        {
            TextColor = cleaned.TextColor?.ToLowerInvariant(),
            BackgroundColor = cleaned.BackgroundColor?.ToLowerInvariant(),
        };
    }
}
