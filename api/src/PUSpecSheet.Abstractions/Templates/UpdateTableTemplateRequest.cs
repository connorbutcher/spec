using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Templates;

public sealed record UpdateTableTemplateRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    public required TemplateOrientation Orientation { get; init; }

    [Range(0, 50)]
    public required int StickyColumnCount { get; init; }
}
