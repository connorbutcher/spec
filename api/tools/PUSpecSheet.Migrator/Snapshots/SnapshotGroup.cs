
namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// A group of a table of a sheet snapshot. <see cref="LinkedGroupReferenceId"/> is the ReferenceID of the group it is
/// linked to, if it is.
/// </summary>
internal sealed record SnapshotGroup
{
    public required int Id { get; init; }

    public required Guid ReferenceId { get; init; }

    public required int TableId { get; init; }

    public required int TableGroupTemplateId { get; init; }

    public required int DisplayOrder { get; init; }

    public required IReadOnlyList<SnapshotWidgetValue> WidgetValues { get; init; }

    public required Guid? LinkedGroupReferenceId { get; init; }
}
