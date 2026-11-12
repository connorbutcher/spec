using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// One place a looked-up value was found, at one version of its sheet, with the rows that belong to it by
/// row identifier, in the same shape as reading rows by identifier.
///
/// When the value is at the top of a set of repeated columns (a part number over a part's columns),
/// <see cref="Column"/> is that set's name, <see cref="Rows"/> are the table's rows, each with the set's
/// values under that name in its <c>columns</c>, and <see cref="Headings"/> are the set's headings.
///
/// Otherwise the value is in one of a row's own cells. <see cref="Rows"/> are then the rows of the cell's
/// section and its sub-sections (or of the whole table when the cell is in the header), and
/// <see cref="Headings"/> are the table's own headings.
/// </summary>
public sealed record PublishedLookupMatchDto
{
    public PublishedLookupMatchDto(
        Guid sheet,
        string phase,
        int sheetType,
        int version,
        DateTime publishedAtUtc,
        Guid table,
        string? title,
        string? column,
        IReadOnlyList<string> headings,
        IReadOnlyDictionary<Guid, PublishedRowValuesDto> rows)
    {
        this.Sheet = sheet;
        this.Phase = phase;
        this.SheetType = sheetType;
        this.Version = version;
        this.PublishedAtUtc = publishedAtUtc;
        this.Table = table;
        this.Title = title;
        this.Column = column;
        this.Headings = headings;
        this.Rows = rows;
    }

    public Guid Sheet { get; init; }

    public string Phase { get; init; }

    public int SheetType { get; init; }

    public int Version { get; init; }

    public DateTime PublishedAtUtc { get; init; }

    public Guid Table { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Column { get; init; }

    public IReadOnlyList<string> Headings { get; init; }

    public IReadOnlyDictionary<Guid, PublishedRowValuesDto> Rows { get; init; }
}
