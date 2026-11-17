namespace PUSpecSheet.Abstractions.Published;

/// <summary>
/// What to read from a published sheet, for selections too long for a query string. The four lists are
/// combined: a table or section brings everything beneath it. With no lists the whole sheet is returned.
/// </summary>
public sealed record PublishedSheetQueryRequest
{
    public IReadOnlyList<Guid>? Tables { get; init; }

    public IReadOnlyList<Guid>? Sections { get; init; }

    public IReadOnlyList<Guid>? Rows { get; init; }

    public IReadOnlyList<Guid>? Cells { get; init; }

    public PublishedSheetShape Shape { get; init; } = PublishedSheetShape.Tree;

    public bool IncludeLabels { get; init; }
}
