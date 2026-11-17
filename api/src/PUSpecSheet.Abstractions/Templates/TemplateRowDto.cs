namespace PUSpecSheet.Abstractions.Templates;

/// <summary>A row of a section with its cells ordered by column.</summary>
public sealed record TemplateRowDto
{
    public required int Id { get; init; }

    public required int DisplayOrder { get; init; }

    public required IReadOnlyList<TemplateCellDto> Cells { get; init; }
}
