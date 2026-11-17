namespace PUSpecSheet.Query.Models.Values;

public sealed record SlimLinkedValue : SlimValue
{
    public required Guid LinkedReferenceId { get; init; }

    public required SlimValue? Value { get; init; }

    public required string TableName { get; init; }

    public required Guid TableReferenceId { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required int SpecSheetTypeId { get; init; }
}
