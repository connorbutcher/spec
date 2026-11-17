namespace PUSpecSheet.Migrator.Snapshots;

internal sealed record SnapshotSection
{
    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TableId { get; init; }

    public int? ParentSectionId { get; init; }

    public int TemplateSectionId { get; init; }

    public int DisplayOrder { get; init; }

    public string? WidgetValues { get; init; }

    public Guid? LinkedSectionPublicId { get; init; }
}
