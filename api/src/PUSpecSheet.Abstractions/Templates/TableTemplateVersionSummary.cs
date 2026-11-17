namespace PUSpecSheet.Abstractions.Templates;

/// <summary>One version of a template, for the version picker. A version in use is read-only.</summary>
public sealed record TableTemplateVersionSummary
{
    public required int Id { get; init; }

    public required int VersionNumber { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required bool IsInUse { get; init; }
}
