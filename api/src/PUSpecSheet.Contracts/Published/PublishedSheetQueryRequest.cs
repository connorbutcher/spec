namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// What to read from a published sheet, for selections too long for a query string. The four lists are
/// combined: a table or section brings everything beneath it. With no lists the whole sheet is returned.
/// </summary>
public sealed record PublishedSheetQueryRequest
{
    public PublishedSheetQueryRequest(
        IReadOnlyList<Guid>? tables,
        IReadOnlyList<Guid>? sections,
        IReadOnlyList<Guid>? rows,
        IReadOnlyList<Guid>? cells,
        PublishedSheetShape shape = PublishedSheetShape.Tree,
        bool includeLabels = false)
    {
        this.Tables = tables;
        this.Sections = sections;
        this.Rows = rows;
        this.Cells = cells;
        this.Shape = shape;
        this.IncludeLabels = includeLabels;
    }

    public IReadOnlyList<Guid>? Tables { get; init; }

    public IReadOnlyList<Guid>? Sections { get; init; }

    public IReadOnlyList<Guid>? Rows { get; init; }

    public IReadOnlyList<Guid>? Cells { get; init; }

    public PublishedSheetShape Shape { get; init; }

    public bool IncludeLabels { get; init; }
}
