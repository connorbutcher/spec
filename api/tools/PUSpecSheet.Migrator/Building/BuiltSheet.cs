using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// A sheet's history built from its copies, ready to save: the sheet with everything added under it, what it must
/// show once saved, and the submissions to save after it.
/// </summary>
internal sealed record BuiltSheet
{
    public required DbSheet Sheet { get; init; }

    public required IReadOnlyList<BuiltVersion> Checks { get; init; }

    public required IReadOnlyList<PlannedSubmission> Submissions { get; init; }

    public required IReadOnlyList<string> Warnings { get; init; }
}
