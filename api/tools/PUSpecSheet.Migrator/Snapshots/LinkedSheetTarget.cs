namespace PUSpecSheet.Migrator.Snapshots;

internal sealed record LinkedSheetTarget
{
    public int? SheetTypeId { get; init; }

    public int? PhaseId { get; init; }

    public Guid? TablePublicId { get; init; }

    public Guid? ItemPublicId { get; init; }
}
