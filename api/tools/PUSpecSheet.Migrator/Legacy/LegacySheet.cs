namespace PUSpecSheet.Migrator.Legacy;

// Every SpecSheets row for one phase and sheet type: each a whole copy of the sheet at one version.
internal sealed record LegacySheet
{
    public int PhaseId { get; init; }

    public int SheetTypeId { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public string Name => $"Phase {PhaseId}, sheet type {SheetTypeId}";
}
