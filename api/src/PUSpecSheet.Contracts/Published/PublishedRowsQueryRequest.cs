namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// The rows to read from a published sheet, for lists too long for a query string. <see cref="Rows"/> are
/// row identifiers; with none every row is returned. <see cref="Columns"/> keeps only the named sets of
/// repeated columns, such as the part numbers "P-1003" and "P-1004".
/// </summary>
public sealed record PublishedRowsQueryRequest(
    IReadOnlyList<Guid>? Rows,
    IReadOnlyList<string>? Columns = null);
