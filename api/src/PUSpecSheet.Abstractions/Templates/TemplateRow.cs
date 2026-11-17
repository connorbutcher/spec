namespace PUSpecSheet.Abstractions.Templates;

/// <summary>A row of a section with its cells ordered by column.</summary>
public sealed record TemplateRow
{
    public required int Id { get; init; }

    public required int DisplayOrder { get; init; }

    public required IReadOnlyList<TemplateCell> Cells { get; init; }
}
