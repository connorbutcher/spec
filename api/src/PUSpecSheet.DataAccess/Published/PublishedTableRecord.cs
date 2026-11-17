namespace PUSpecSheet.DataAccess.Published;

/// <summary>A sheet table as it stood at a version: where it sat and whether it had been removed.</summary>
public sealed record PublishedTableRecord
{
    public required int Id { get; init; }

    public required Guid PublicId { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsDeleted { get; init; }

    public string? Title { get; init; }
}
