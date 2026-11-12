namespace PUSpecSheet.Contracts.Sheets;

/// <summary>A kind of row that can be added to a section, named after its first caption.</summary>
public sealed record AddableRowDto
{
    public AddableRowDto(int templateRowId, string label)
    {
        this.TemplateRowId = templateRowId;
        this.Label = label;
    }

    public int TemplateRowId { get; init; }

    public string Label { get; init; }
}
