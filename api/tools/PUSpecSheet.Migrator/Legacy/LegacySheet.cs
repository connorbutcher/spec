namespace PUSpecSheet.Migrator.Legacy;

/// <summary>A sheet in the legacy layout: one per phase and sheet type, its versions held as whole copies.</summary>
internal sealed record LegacySheet
{
    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int PhaseId { get; init; }

    public int SheetTypeId { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
