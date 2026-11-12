namespace PUSpecSheet.Application.Published;

/// <summary>A sheet column block as it stood at a version: where it sat and whether it had been removed.</summary>
public sealed record PublishedColumnBlockRecord
{
    public PublishedColumnBlockRecord(int id, Guid publicId, int tableId, int displayOrder, bool isDeleted, string name)
    {
        this.Id = id;
        this.PublicId = publicId;
        this.TableId = tableId;
        this.DisplayOrder = displayOrder;
        this.IsDeleted = isDeleted;
        this.Name = name;
    }

    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TableId { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsDeleted { get; init; }

    public string Name { get; init; }
}
