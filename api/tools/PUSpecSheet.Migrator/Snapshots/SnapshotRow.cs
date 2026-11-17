namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// A row of a group of a sheet snapshot.
/// </summary>
internal sealed record SnapshotRow
{
    public required int Id { get; init; }

    public required Guid ReferenceId { get; init; }

    public required int GroupId { get; init; }

    public required int TableRowTemplateId { get; init; }

    public required int DisplayOrder { get; init; }
}
