namespace PUSpecSheet.Abstractions.Requests.Access;

public record UpdateSpecSheetPhaseObserverRequest
{
    public required int Id { get; init; }

    public required bool IsApprover { get; init; }
}
