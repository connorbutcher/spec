using System.Globalization;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// Every SpecSheets row for one phase and sheet type: each a whole copy of the sheet at one version.
/// </summary>
internal sealed record LegacySheet
{
    public required int PhaseId { get; init; }

    public required int SheetTypeId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public string Name => string.Create(CultureInfo.InvariantCulture, $"Phase {PhaseId}, sheet type {SheetTypeId}");
}
