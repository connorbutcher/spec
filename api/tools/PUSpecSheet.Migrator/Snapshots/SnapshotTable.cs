namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>A table on a sheet snapshot.</summary>
internal sealed record SnapshotTable
{
    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TableTemplateVersionId { get; init; }

    public string? Title { get; init; }

    public int DisplayOrder { get; init; }
}
