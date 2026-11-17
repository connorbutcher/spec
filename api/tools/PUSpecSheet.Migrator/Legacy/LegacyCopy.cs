using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// One SpecSheets row: a published version, or the working copy when <see cref="PublishedDateTime"/> is null.
/// </summary>
internal sealed record LegacyCopy
{
    public required int Id { get; init; }

    public required DateTime? PublishedDateTime { get; init; }

    public required int? PublishedByUserId { get; init; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required string? ChangeLog { get; init; }

    public required int? LegacySpecSheetId { get; init; }

    public required bool HasBeenMigrated { get; init; }

    public required SheetSnapshot Snapshot { get; init; }

    public required IReadOnlyList<LegacyWorkflowStep> Workflow { get; init; }

    public bool IsWorkingCopy => PublishedDateTime is null;

    /// <summary>
    /// Who published it, or who is editing it.
    /// </summary>
    public int UserId => PublishedByUserId ?? CreatedByUserId;
}
