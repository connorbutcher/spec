namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// What a linked sheet cell points at: another sheet, by type and phase, and a table and item in it, by ReferenceID.
/// </summary>
internal sealed record LinkedSheetTarget
{
    public required int? SheetTypeId { get; init; }

    public required int? PhaseId { get; init; }

    public required Guid? TableReferenceId { get; init; }

    public required Guid? ItemReferenceId { get; init; }
}
