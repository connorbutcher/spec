namespace PUSpecSheet.DataAccess.Published;

public sealed record PublishedColumnBlockRecord
{
    public required int Id { get; init; }

    public required Guid PublicId { get; init; }

    public required int TableId { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsDeleted { get; init; }

    public required string Name { get; init; }
}
