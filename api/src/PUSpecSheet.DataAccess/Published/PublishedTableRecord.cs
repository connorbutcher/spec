namespace PUSpecSheet.DataAccess.Published;

/// <summary>A sheet table as it stood at a version: where it sat and whether it had been removed.</summary>
public sealed record PublishedTableRecord
{
    public PublishedTableRecord(int id, Guid publicId, int displayOrder, bool isDeleted, string? title)
    {
        Id = id;
        PublicId = publicId;
        DisplayOrder = displayOrder;
        IsDeleted = isDeleted;
        Title = title;
    }

    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsDeleted { get; init; }

    public string? Title { get; init; }
}
