namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// What to read from a published sheet, for selections too long for a query string. The four lists are
/// combined: a table or section brings everything beneath it. With no lists the whole sheet is returned.
/// </summary>
public sealed record PublishedSheetQueryRequest(
    IReadOnlyList<Guid>? Tables,
    IReadOnlyList<Guid>? Sections,
    IReadOnlyList<Guid>? Rows,
    IReadOnlyList<Guid>? Cells,
    PublishedSheetShape Shape = PublishedSheetShape.Tree,
    bool IncludeLabels = false);
