namespace PUSpecSheet.Abstractions.Domain.Values;

public record LinkedSpecSheetValue : CellValue
{
    public required Guid? TableReferenceId { get; set; }

    public required int? LinkedSpecSheetTypeId { get; set; }

    public required int? LinkedSpecSheetPhaseId { get; set; }

    public required Guid? LinkedReferenceId { get; set; }
}
