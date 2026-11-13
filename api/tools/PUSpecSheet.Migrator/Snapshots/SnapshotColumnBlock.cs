namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>A copy of a template column block on a table of a sheet snapshot.</summary>
internal sealed record SnapshotColumnBlock
{
    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TableId { get; init; }

    public int TemplateColumnBlockId { get; init; }

    public int DisplayOrder { get; init; }
}
