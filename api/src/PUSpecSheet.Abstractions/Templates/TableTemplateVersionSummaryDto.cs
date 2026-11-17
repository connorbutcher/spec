namespace PUSpecSheet.Abstractions.Templates;

/// <summary>One version of a template, for the version picker. A version in use is read-only.</summary>
public sealed record TableTemplateVersionSummaryDto
{
    public TableTemplateVersionSummaryDto(int id, int versionNumber, DateTime createdAtUtc, bool isInUse)
    {
        Id = id;
        VersionNumber = versionNumber;
        CreatedAtUtc = createdAtUtc;
        IsInUse = isInUse;
    }

    public int Id { get; init; }

    public int VersionNumber { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public bool IsInUse { get; init; }
}
