using System.Text.Json.Serialization;

namespace PUSpecSheet.Domain.CellTypes.InstanceSettings;

/// <summary>
/// The settings of one cell that are chosen on the sheet, not in the template: which table a linked
/// dropdown takes its choices from, for example. They belong to a row revision, so they are drafted,
/// published and read back exactly like the cell's value. Stored and sent as JSON, with <c>kind</c>
/// naming the derived type, so a new setting is a new property or a new derived type and never a new column.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(LinkedDropdownInstanceSettings), nameof(CellKind.LinkedDropdown))]
public abstract record CellInstanceSettings
{
    /// <summary>The kind these settings belong to. Written to JSON as the type discriminator.</summary>
    [JsonIgnore]
    public CellKind Kind => GetKind();

    /// <summary>Whether nothing is set, so there is nothing worth storing.</summary>
    [JsonIgnore]
    public bool IsEmpty => this == CellInstanceSettingsCatalog.CreateEmpty(Kind);

    /// <summary>
    /// The kind each derived type is for. A method rather than an overridden property, so the
    /// <see cref="JsonIgnoreAttribute"/> on <see cref="Kind"/> keeps it from clashing with the discriminator.
    /// </summary>
    protected abstract CellKind GetKind();
}
