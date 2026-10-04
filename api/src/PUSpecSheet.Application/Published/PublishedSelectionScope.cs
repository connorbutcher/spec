namespace PUSpecSheet.Application.Published;

/// <summary>
/// A selection turned into what the cell query filters on: the tables and sections (with their
/// sub-sections) by database id, and the rows and cells by their public identifiers.
/// </summary>
public sealed record PublishedSelectionScope(
    IReadOnlyList<int> TableIds,
    IReadOnlyList<int> SectionIds,
    IReadOnlyList<Guid> RowPublicIds,
    IReadOnlyList<Guid> CellPublicIds)
{
    /// <summary>False when nothing the caller named is on the sheet, so there is nothing to read.</summary>
    public bool HasAny => TableIds.Count + SectionIds.Count + RowPublicIds.Count + CellPublicIds.Count > 0;
}
