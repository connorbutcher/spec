namespace PUSpecSheet.Abstractions.Templates;

/// <summary>A row of a section with its cells ordered by column.</summary>
public sealed record TemplateRowDto
{
    public TemplateRowDto(int id, int displayOrder, IReadOnlyList<TemplateCellDto> cells)
    {
        Id = id;
        DisplayOrder = displayOrder;
        Cells = cells;
    }

    public int Id { get; init; }

    public int DisplayOrder { get; init; }

    public IReadOnlyList<TemplateCellDto> Cells { get; init; }
}
