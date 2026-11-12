namespace PUSpecSheet.Application.Persistence.Templates;

/// <summary>Which template a version belongs to, and its number there.</summary>
public sealed record TemplateVersionKey(int TableTemplateId, int VersionNumber);
