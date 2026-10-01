namespace PUSpecSheet.Contracts.Sheets;

/// <summary>A table template of the sheet's type that can be added as a table, at its latest version.</summary>
public sealed record AvailableTemplateDto(int Id, string Name, int LatestVersionNumber);
