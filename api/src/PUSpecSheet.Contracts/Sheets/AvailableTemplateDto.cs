namespace PUSpecSheet.Contracts.Sheets;

/// <summary>A table template of the sheet's type that can be added as a table, at its latest version.</summary>
public sealed record AvailableTemplateDto
{
    public AvailableTemplateDto(int id, string name, int latestVersionNumber)
    {
        this.Id = id;
        this.Name = name;
        this.LatestVersionNumber = latestVersionNumber;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public int LatestVersionNumber { get; init; }
}
