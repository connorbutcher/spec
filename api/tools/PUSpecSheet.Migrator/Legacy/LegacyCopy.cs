using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// One whole copy of a legacy sheet: a published version, or the working copy being edited when
/// <see cref="PublishedAtUtc"/> is null. <see cref="UserId"/> is who published it, or who is editing it.
/// </summary>
internal sealed record LegacyCopy
{
    public int Id { get; init; }

    public int VersionNumber { get; init; }

    public DateTime? PublishedAtUtc { get; init; }

    public int UserId { get; init; }

    public string? Note { get; init; }

    public SheetSnapshot Snapshot { get; init; } = new();

    public bool IsWorkingCopy => PublishedAtUtc is null;
}
