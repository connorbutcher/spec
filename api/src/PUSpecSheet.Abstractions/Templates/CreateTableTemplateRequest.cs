using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Templates;

/// <summary>Adds a table template to the end of a sheet type's list.</summary>
public sealed record CreateTableTemplateRequest
{
    public required int SheetTypeId { get; init; }

    [Required, MaxLength(100)]
    public required string Name { get; init; }

    public required TemplateOrientation Orientation { get; init; }
}
