namespace PUSpecSheet.Api.Database.Schema.Legacy.Values;

public record DbLinkedSpecSheetValue
{
    public required int Id { get; init; }

    public required int SpecSheetTableCellId { get; set; }

    public required Guid? TableReferenceId { get; set; }

    public required int? LinkedSpecSheetTypeId { get; set; }

    public required int? LinkedSpecSheetPhaseId { get; set; }

    public required Guid? LinkedReferenceId { get; set; }
}
