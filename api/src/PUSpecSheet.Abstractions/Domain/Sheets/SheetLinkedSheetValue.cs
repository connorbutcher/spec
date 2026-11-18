namespace PUSpecSheet.Abstractions.Domain.Sheets;

public record SheetLinkedSheetValue : SheetCellValue
{
    public required Guid? TableReferenceId { get; init; }

    public required int? LinkedSpecSheetTypeId { get; init; }

    public required int? LinkedSpecSheetPhaseId { get; init; }

    public required Guid? LinkedReferenceId { get; init; }
}
