namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>A row of a section on a sheet snapshot.</summary>
internal sealed record SnapshotRow
{
    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int SectionId { get; init; }

    public int TemplateRowId { get; init; }

    public int DisplayOrder { get; init; }
}
