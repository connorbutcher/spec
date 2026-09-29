using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.CellTypes;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Application.CellTypes;

/// <summary>
/// Copies a <see cref="SaveCellTypeRequest"/> onto a cell type, keeping only the configuration and
/// options its kind uses.
/// </summary>
internal static class CellTypeSettings
{
    public static void Apply(SaveCellTypeRequest request, CellType cellType)
    {
        cellType.Name = request.Name.Trim();
        cellType.Kind = request.Kind;
        cellType.Description = CellSettingText.NullIfBlank(request.Description);
        cellType.Configuration = CleanConfiguration(request.Kind, request.Configuration);
        cellType.Style = CleanStyle(request.Style);

        var options = request.Kind.IsDropdown() ? CellTypeOptions.Clean(request.Kind, request.Options) : [];
        CellTypeOptions.Sync(cellType, options);
    }

    /// <summary>
    /// Checks a configuration and trims its text. One for another kind is dropped, so changing a cell
    /// type's kind starts the new kind with nothing set.
    /// </summary>
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
