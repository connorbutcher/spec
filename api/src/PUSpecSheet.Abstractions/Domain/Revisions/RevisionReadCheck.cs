namespace PUSpecSheet.Abstractions.Domain.Revisions;

/// <summary>
/// How published spec sheets read from the revisions compare with the same sheets read from their copies.
/// </summary>
public sealed record RevisionReadCheck
{
    public required int Checked { get; init; }

    public required int Matched { get; init; }

    /// <summary>
    /// Published copies with no version in the revisions: not migrated yet, or published since the migrator last ran.
    /// </summary>
    public required IReadOnlyList<int> NotMigrated { get; init; }

    public required IReadOnlyList<RevisionReadMismatch> Mismatches { get; init; }
}
