using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// One row of a published sheet at one version. <see cref="Section"/>, <see cref="Values"/> and
/// <see cref="Columns"/> are as in <see cref="PublishedRowValuesDto"/>.
/// </summary>
public sealed record PublishedSheetRowDto(
    Guid Sheet,
    int Version,
    DateTime PublishedAtUtc,
    Guid Row,
    string Section,
    IReadOnlyList<object?> Values,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyDictionary<string, IReadOnlyList<object?>>? Columns);
