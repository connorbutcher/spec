namespace PUSpecSheet.Application.Published.Sheets;

/// <summary>
/// A selection turned into what the cell query filters on: the tables and sections (with their
/// sub-sections) by database id, and the rows and cells by their public identifiers.
/// </summary>
public sealed record PublishedSelectionScope
{
    public PublishedSelectionScope(
        IReadOnlyList<int> tableIds,
        IReadOnlyList<int> sectionIds,
        IReadOnlyList<Guid> rowPublicIds,
        IReadOnlyList<Guid> cellPublicIds)
    {
        this.TableIds = tableIds;
        this.SectionIds = sectionIds;
        this.RowPublicIds = rowPublicIds;
        this.CellPublicIds = cellPublicIds;
    }

    public IReadOnlyList<int> TableIds { get; init; }

    public IReadOnlyList<int> SectionIds { get; init; }

    public IReadOnlyList<Guid> RowPublicIds { get; init; }

    public IReadOnlyList<Guid> CellPublicIds { get; init; }

    /// <summary>False when nothing the caller named is on the sheet, so there is nothing to read.</summary>
    public bool HasAny => TableIds.Count + SectionIds.Count + RowPublicIds.Count + CellPublicIds.Count > 0;
}
