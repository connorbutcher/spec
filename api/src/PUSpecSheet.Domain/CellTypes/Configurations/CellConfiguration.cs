using System.Text.Json.Serialization;

namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>
/// The kind-specific settings of a cell type, or the part of them a template cell overrides. Every
/// setting is nullable: on a cell type null means "not set", and on a cell override it means "use the
/// cell type's value". Stored and sent as JSON, with <c>kind</c> naming the derived type.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(HeadingCellConfiguration), nameof(CellKind.Heading))]
[JsonDerivedType(typeof(GroupCellConfiguration), nameof(CellKind.Group))]
[JsonDerivedType(typeof(TextCellConfiguration), nameof(CellKind.Text))]
[JsonDerivedType(typeof(NumberCellConfiguration), nameof(CellKind.Number))]
[JsonDerivedType(typeof(DateCellConfiguration), nameof(CellKind.Date))]
[JsonDerivedType(typeof(CheckboxCellConfiguration), nameof(CellKind.Checkbox))]
[JsonDerivedType(typeof(TextDropdownCellConfiguration), nameof(CellKind.TextDropdown))]
[JsonDerivedType(typeof(NumberDropdownCellConfiguration), nameof(CellKind.NumberDropdown))]
public abstract record CellConfiguration
{
    /// <summary>The kind these settings belong to. Written to JSON as the type discriminator.</summary>
    [JsonIgnore]
    public CellKind Kind => GetKind();

    /// <summary>
    /// These settings with every value <paramref name="cellOverride"/> sets laid on top. Overrides for a
    /// different kind are ignored, so a cell whose type changed kind falls back to the defaults.
    /// </summary>
    public abstract CellConfiguration Apply(CellConfiguration? cellOverride);

    /// <summary>Why these settings are invalid, or null when they're fine.</summary>
    public virtual string? Validate()
    {
        return null;
    }

    /// <summary>
    /// The kind each derived type is for. A method rather than an overridden property, so the
    /// <see cref="JsonIgnoreAttribute"/> on <see cref="Kind"/> keeps it from clashing with the discriminator.
    /// </summary>
    protected abstract CellKind GetKind();
}
