namespace PUSpecSheet.Contracts.Templates;

/// <summary>One version of a template, for the version picker. A version in use is read-only.</summary>
public sealed record TableTemplateVersionSummaryDto(int Id, int VersionNumber, DateTime CreatedAtUtc, bool IsInUse);
