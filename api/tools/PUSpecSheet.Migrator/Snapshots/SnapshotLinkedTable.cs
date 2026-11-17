namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// Another sheet's table shown on a sheet snapshot, by the table's ReferenceID.
/// </summary>
internal sealed record SnapshotLinkedTable
{
    public required Guid TableReferenceId { get; init; }

    public required int DisplayOrder { get; init; }

    public required int LinkedByUserId { get; init; }

    public required DateTime LinkedDateTime { get; init; }
}
