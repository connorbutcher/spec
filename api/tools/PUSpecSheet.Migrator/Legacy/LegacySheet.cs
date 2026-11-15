namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// A sheet in the legacy layout: every <c>PUSpecSheet</c> row for one phase and sheet type, each a whole copy of
/// the sheet at one version. <see cref="PublicId"/> is the latest copy's, which the migrated sheet keeps.
/// </summary>
internal sealed record LegacySheet
{
    public int PhaseId { get; init; }

    public int SheetTypeId { get; init; }

    public Guid PublicId { get; init; }

    /// <summary>When the first copy was published.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>How the logs refer to the sheet.</summary>
    public string Name => $"Phase {PhaseId}, sheet type {SheetTypeId}";
}
