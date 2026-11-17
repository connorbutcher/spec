using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Migrator.Building;

internal sealed record BuiltSheet
{
    public required DbSheet Sheet { get; init; }

    public required IReadOnlyList<BuiltVersion> Checks { get; init; }

    public required IReadOnlyList<PlannedSubmission> Submissions { get; init; }

    public required IReadOnlyList<string> Warnings { get; init; }
}
