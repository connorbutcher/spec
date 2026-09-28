namespace PUSpecSheet.Contracts.Templates;

/// <summary>A row of a section with its cells ordered by column.</summary>
public sealed record TemplateRowDto(int Id, int DisplayOrder, IReadOnlyList<TemplateCellDto> Cells);
