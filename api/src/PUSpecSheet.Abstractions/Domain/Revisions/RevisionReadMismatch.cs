namespace PUSpecSheet.Abstractions.Domain.Revisions;

/// <summary>
/// A published spec sheet that reads differently from the revisions, or couldn't be read from them.
/// </summary>
public sealed record RevisionReadMismatch
{
    public required int SpecSheetId { get; init; }

    public required string Difference { get; init; }
}
