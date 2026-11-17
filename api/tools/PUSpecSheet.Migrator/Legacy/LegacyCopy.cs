using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Legacy;

// One SpecSheets row: a published version, or the working copy when PublishedAtUtc is null.
internal sealed record LegacyCopy
{
    public int Id { get; init; }

    public DateTime? PublishedAtUtc { get; init; }

    public int? PublishedByUserId { get; init; }

    public int CreatedByUserId { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public string? ChangeLog { get; init; }

    public int? LegacySpecSheetId { get; init; }

    public bool HasBeenMigrated { get; init; }

    public SheetSnapshot Snapshot { get; init; } = new();

    public IReadOnlyList<LegacyWorkflowStep> Workflow { get; init; } = [];

    public bool IsWorkingCopy => PublishedAtUtc is null;

    // Who published it, or who is editing it.
    public int UserId => PublishedByUserId ?? CreatedByUserId;
}
