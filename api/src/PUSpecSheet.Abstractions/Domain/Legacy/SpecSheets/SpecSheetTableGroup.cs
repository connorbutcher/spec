using PUSpecSheet.Abstractions.Domain.Access;

namespace PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;

public record SpecSheetTableGroup
{
    public required int Id { get; init; }

    public required int SpecSheetTableId { get; init; }

    public required int TableGroupTemplateId { get; init; }

    public required int? LinkedSpecSheetTableGroupId { get; set; }

    public required int Order { get; set; }

    public required Guid ReferenceId { get; init; }

    public required bool IsDeleted { get; set; }

    public required ICollection<SpecSheetTableRow> Rows { get; set; } = [];

    public required ICollection<SpecSheetTableGroupWidget> Widgets { get; set; } = [];

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual User? CreatedByUser { get; init; }

    public int? LastModifiedByUserId { get; set; }

    public DateTime? LastModifiedDateTime { get; set; }

    public virtual User? LastModifiedByUser { get; init; }
}
