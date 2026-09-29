using System.Text.RegularExpressions;

namespace PUSpecSheet.Domain.CellTypes.Styles;

/// <summary>
/// How a cell looks, whatever its kind. Every value is nullable: on a cell type null means "the app's
/// default", and on a cell override it means "use the cell type's value".
/// </summary>
public sealed partial record CellStyle
{
    public bool? Bold { get; init; }

    public bool? Italic { get; init; }

    public CellTextAlign? Align { get; init; }

    /// <summary>A hex colour such as "#1f2937".</summary>
    public string? TextColor { get; init; }

    /// <summary>A hex colour such as "#f1f5f9".</summary>
    public string? BackgroundColor { get; init; }

    /// <summary>Whether this style sets nothing, so as an override it changes nothing.</summary>
    public bool IsEmpty()
    {
        return this == new CellStyle();
    }

    /// <summary>This style with every value <paramref name="overrides"/> sets laid on top.</summary>
    public CellStyle Apply(CellStyle? overrides)
    {
        if (overrides is null)
        {
            return this;
        }

        return new CellStyle
        {
            Bold = overrides.Bold ?? Bold,
            Italic = overrides.Italic ?? Italic,
            Align = overrides.Align ?? Align,
            TextColor = overrides.TextColor ?? TextColor,
            BackgroundColor = overrides.BackgroundColor ?? BackgroundColor,
        };
    }

    /// <summary>Why this style is invalid, or null when it's fine.</summary>
    public string? Validate()
    {
        if (!IsColor(TextColor) || !IsColor(BackgroundColor))
        {
            return "Colours must be hex values such as #1f2937.";
        }

        return null;
    }

    private static bool IsColor(string? value)
    {
        return value is null || HexColor().IsMatch(value);
    }

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColor();
}
