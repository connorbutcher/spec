namespace PUSpecSheet.Application.Published;

/// <summary>A sheet column block as it stood at a version: where it sat and whether it had been removed.</summary>
public sealed record PublishedColumnBlockRecord(
    int Id,
    Guid PublicId,
    int TableId,
    int DisplayOrder,
    bool IsDeleted,
    string Name);
