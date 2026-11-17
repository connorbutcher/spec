using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Templates;

public sealed record UpdateTemplateCellRequest
{
    public required int CellTypeId { get; init; }

    [Range(1, 100)]
    public required int Column { get; init; }

    [Range(1, 100)]
    public required int RowSpan { get; init; }

    [Range(1, 100)]
    public required int ColumnSpan { get; init; }

    [MaxLength(200)]
    public string? Caption { get; init; }

    public required bool IsRequired { get; init; }
}
