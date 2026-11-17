namespace PUSpecSheet.DataAccess.Published.Sheets;

public sealed record PublishedSelectionScope
{
    public required IReadOnlyList<int> TableIds { get; init; }

    public required IReadOnlyList<int> SectionIds { get; init; }

    public required IReadOnlyList<Guid> RowPublicIds { get; init; }

    public required IReadOnlyList<Guid> CellPublicIds { get; init; }

    public bool HasAny => TableIds.Count + SectionIds.Count + RowPublicIds.Count + CellPublicIds.Count > 0;
}
