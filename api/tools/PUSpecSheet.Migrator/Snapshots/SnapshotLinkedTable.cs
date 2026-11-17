namespace PUSpecSheet.Migrator.Snapshots;

internal sealed record SnapshotLinkedTable
{
    public required Guid LinkedTablePublicId { get; init; }

    public int DisplayOrder { get; init; }

    public int LinkedByUserId { get; init; }

    public DateTime LinkedAtUtc { get; init; }
}
