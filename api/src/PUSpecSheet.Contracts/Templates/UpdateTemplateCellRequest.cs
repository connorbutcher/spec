using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

public sealed record UpdateTemplateCellRequest(
    int CellTypeId,
    [Range(1, 100)] int Column,
    [Range(1, 100)] int RowSpan,
    [Range(1, 100)] int ColumnSpan,
    [MaxLength(200)] string? Caption,
    bool IsRequired);
