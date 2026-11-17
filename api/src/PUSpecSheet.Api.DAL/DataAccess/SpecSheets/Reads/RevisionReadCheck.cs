namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

/// <summary>
/// How published spec sheets read from the revisions compare with the same sheets read from their copies.
/// </summary>
public record RevisionReadCheck
{
    public required int Checked { get; init; }

    public required int Matched { get; init; }

    /// <summary>
    /// Published copies with no version in the revisions: not migrated yet, or published since the migrator last ran.
    /// </summary>
    public required IReadOnlyList<int> NotMigrated { get; init; }

    public required IReadOnlyList<RevisionReadMismatch> Mismatches { get; init; }
}

/// <summary>
/// A published spec sheet that reads differently from the revisions, or couldn't be read from them.
/// </summary>
public record RevisionReadMismatch
{
    public required int SpecSheetId { get; init; }

    public required string Difference { get; init; }
}
