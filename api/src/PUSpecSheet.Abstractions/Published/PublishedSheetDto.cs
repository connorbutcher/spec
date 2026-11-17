using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Published;

/// <summary>
/// A sheet's published values at one version, for another application to read. Only one of
/// <see cref="Tables"/> (the tree shape) and <see cref="Cells"/> (the flat shape) is set. A version never
/// changes once published, so the same request for the same version always gives the same answer.
/// </summary>
public sealed record PublishedSheetDto
{
    public PublishedSheetDto(
        Guid sheet,
        int version,
        DateTime publishedAtUtc,
        IReadOnlyList<PublishedTableDto>? tables,
        IReadOnlyDictionary<Guid, object>? cells,
        IReadOnlyList<Guid> missing)
    {
        Sheet = sheet;
        Version = version;
        PublishedAtUtc = publishedAtUtc;
        Tables = tables;
        Cells = cells;
        Missing = missing;
    }

    public Guid Sheet { get; init; }

    public int Version { get; init; }

    public DateTime PublishedAtUtc { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<PublishedTableDto>? Tables { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<Guid, object>? Cells { get; init; }

    public IReadOnlyList<Guid> Missing { get; init; }
}
