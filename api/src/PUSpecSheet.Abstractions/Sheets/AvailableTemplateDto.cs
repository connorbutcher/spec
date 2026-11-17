namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>A table template of the sheet's type that can be added as a table, at its latest version.</summary>
public sealed record AvailableTemplateDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required int LatestVersionNumber { get; init; }
}
