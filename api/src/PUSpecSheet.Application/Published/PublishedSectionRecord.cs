namespace PUSpecSheet.Application.Published;

/// <summary>A sheet section as it stood at a version: where it sat and whether it had been removed.</summary>
public sealed record PublishedSectionRecord(
    int Id,
    Guid PublicId,
    int TableId,
    int? ParentSectionId,
    int DisplayOrder,
    bool IsDeleted,
    string Name);
