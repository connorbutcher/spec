namespace PUSpecSheet.DataAccess.Published;

/// <summary>A sheet section as it stood at a version: where it sat and whether it had been removed.</summary>
public sealed record PublishedSectionRecord
{
    public PublishedSectionRecord(int id, Guid publicId, int tableId, int? parentSectionId, int displayOrder, bool isDeleted, string name)
    {
        Id = id;
        PublicId = publicId;
        TableId = tableId;
        ParentSectionId = parentSectionId;
        DisplayOrder = displayOrder;
        IsDeleted = isDeleted;
        Name = name;
    }

    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TableId { get; init; }

    public int? ParentSectionId { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsDeleted { get; init; }

    public string Name { get; init; }
}
