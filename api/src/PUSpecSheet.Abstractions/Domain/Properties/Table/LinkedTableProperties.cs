namespace PUSpecSheet.Abstractions.Domain.Properties.Table;

public record LinkedTableProperties : ITableProperties
{
    public required int? SpecSheetPhaseId { get; init; }

    public required int? SpecSheetTypeId { get; init; }

    public required Guid? TableReferenceId { get; init; }

    public required bool LockToLatestPublished { get; init; } = false;
}
