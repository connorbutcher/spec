namespace PUSpecSheet.Migrator.Snapshots;

internal sealed record SnapshotTable
{
    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TableTemplateVersionId { get; init; }

    public string? Title { get; init; }

    public int DisplayOrder { get; init; }

    // Canonical JSON (see SnapshotJson), so equal contents compare equal as text.
    public string? WidgetValues { get; init; }

    public string? Notes { get; init; }
}
