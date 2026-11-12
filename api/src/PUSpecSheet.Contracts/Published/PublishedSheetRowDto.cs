using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// One row of a published sheet at one version. <see cref="Section"/>, <see cref="Values"/> and
/// <see cref="Columns"/> are as in <see cref="PublishedRowValuesDto"/>.
/// </summary>
public sealed record PublishedSheetRowDto
{
    public PublishedSheetRowDto(
        Guid sheet,
        int version,
        DateTime publishedAtUtc,
        Guid row,
        string section,
        IReadOnlyList<object?> values,
        IReadOnlyDictionary<string, IReadOnlyList<object?>>? columns)
    {
        this.Sheet = sheet;
        this.Version = version;
        this.PublishedAtUtc = publishedAtUtc;
        this.Row = row;
        this.Section = section;
        this.Values = values;
        this.Columns = columns;
    }

    public Guid Sheet { get; init; }

    public int Version { get; init; }

    public DateTime PublishedAtUtc { get; init; }

    public Guid Row { get; init; }

    public string Section { get; init; }

    public IReadOnlyList<object?> Values { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, IReadOnlyList<object?>>? Columns { get; init; }
}
