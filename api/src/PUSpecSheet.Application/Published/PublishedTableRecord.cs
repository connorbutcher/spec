namespace PUSpecSheet.Application.Published;

/// <summary>A sheet table as it stood at a version: where it sat and whether it had been removed.</summary>
public sealed record PublishedTableRecord(int Id, Guid PublicId, int DisplayOrder, bool IsDeleted, string? Title);
