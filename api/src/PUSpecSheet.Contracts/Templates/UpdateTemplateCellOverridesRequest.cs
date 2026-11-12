using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Replaces what a cell changes from its cell type's defaults. Null, or a value that sets nothing,
/// goes back to the defaults. <see cref="ConfigurationOverride"/> must be for the cell type's kind.
/// </summary>
public sealed record UpdateTemplateCellOverridesRequest
{
    public UpdateTemplateCellOverridesRequest(CellConfiguration? configurationOverride, CellStyle? styleOverride)
    {
        this.ConfigurationOverride = configurationOverride;
        this.StyleOverride = styleOverride;
    }

    public CellConfiguration? ConfigurationOverride { get; init; }

    public CellStyle? StyleOverride { get; init; }
}
