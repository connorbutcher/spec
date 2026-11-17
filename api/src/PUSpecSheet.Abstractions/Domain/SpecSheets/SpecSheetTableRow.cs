using PUSpecSheet.Abstractions.Domain.Access;

namespace PUSpecSheet.Abstractions.Domain.SpecSheets;

public record SpecSheetTableRow
{
    public required int Id { get; init; }

    public required int SpecSheetTableGroupId { get; init; }

    public required int TableRowTemplateId { get; init; }

    public required int RowNumber { get; set; }

    public required Guid ReferenceId { get; init; }

    public required bool IsDeleted { get; set; }

    public required ICollection<SpecSheetTableCell> Cells { get; init; } = [];

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual User? CreatedByUser { get; init; }

    public int? LastModifiedByUserId { get; set; }

    public DateTime? LastModifiedDateTime { get; set; }

    public virtual User? LastModifiedByUser { get; init; }
}
