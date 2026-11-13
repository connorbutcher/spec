namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>A section of a table on a sheet snapshot, inside another section when <see cref="ParentSectionId"/> is set.</summary>
internal sealed record SnapshotSection
{
    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TableId { get; init; }

    public int? ParentSectionId { get; init; }

    public int TemplateSectionId { get; init; }

    public int DisplayOrder { get; init; }
}
