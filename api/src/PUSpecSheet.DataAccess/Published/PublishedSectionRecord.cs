namespace PUSpecSheet.DataAccess.Published;

/// <summary>A sheet section as it stood at a version: where it sat and whether it had been removed.</summary>
public sealed record PublishedSectionRecord
{
    public required int Id { get; init; }

    public required Guid PublicId { get; init; }

    public required int TableId { get; init; }

    public int? ParentSectionId { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsDeleted { get; init; }

    public required string Name { get; init; }
}
