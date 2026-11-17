namespace PUSpecSheet.Abstractions.Requests.Access;

public record CreateSpecSheetPhaseObserverRequest
{
    public required int SpecSheetPhaseId { get; init; }

    public required int UserId { get; init; }

    public required bool IsApprover { get; init; }
}
