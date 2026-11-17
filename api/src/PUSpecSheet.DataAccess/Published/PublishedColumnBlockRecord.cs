namespace PUSpecSheet.DataAccess.Published;

/// <summary>A sheet column block as it stood at a version: where it sat and whether it had been removed.</summary>
public sealed record PublishedColumnBlockRecord
{
    public PublishedColumnBlockRecord(int id, Guid publicId, int tableId, int displayOrder, bool isDeleted, string name)
    {
        Id = id;
        PublicId = publicId;
        TableId = tableId;
        DisplayOrder = displayOrder;
        IsDeleted = isDeleted;
        Name = name;
    }

    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TableId { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsDeleted { get; init; }

    public string Name { get; init; }
}
