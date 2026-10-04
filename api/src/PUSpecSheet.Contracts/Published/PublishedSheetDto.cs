using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// A sheet's published values at one version, for another application to read. Only one of
/// <see cref="Tables"/> (the tree shape) and <see cref="Cells"/> (the flat shape) is set. A version never
/// changes once published, so the same request for the same version always gives the same answer.
/// </summary>
public sealed record PublishedSheetDto(
    Guid Sheet,
    int Version,
    DateTime PublishedAtUtc,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<PublishedTableDto>? Tables,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyDictionary<Guid, object>? Cells,
    IReadOnlyList<Guid> Missing);
