using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// A cell's value on a published sheet: a number, text, true or false, or a date (yyyy-MM-dd). A dropdown
/// gives the chosen option's text, or its number for a number dropdown. <see cref="Column"/> is set for a
/// cell in a set of repeated columns and identifies that set; <see cref="Caption"/> only when labels are
/// asked for.
/// </summary>
public sealed record PublishedCellDto(
    Guid Id,
    object Value,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Guid? Column,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Caption);
