using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Templates;

public sealed record UpdateTemplateCellRequest
{
    public UpdateTemplateCellRequest(
        int cellTypeId,
        [Range(1, 100)] int column,
        [Range(1, 100)] int rowSpan,
        [Range(1, 100)] int columnSpan,
        [MaxLength(200)] string? caption,
        bool isRequired)
    {
        CellTypeId = cellTypeId;
        Column = column;
        RowSpan = rowSpan;
        ColumnSpan = columnSpan;
        Caption = caption;
        IsRequired = isRequired;
    }

    public int CellTypeId { get; init; }

    public int Column { get; init; }

    public int RowSpan { get; init; }

    public int ColumnSpan { get; init; }

    public string? Caption { get; init; }

    public bool IsRequired { get; init; }
}
