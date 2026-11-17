namespace PUSpecSheet.Abstractions.Domain.Access;

public record SpecSheetPhaseObserver
{
    public required int Id { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required int UserId { get; init; }

    public required bool IsApprover { get; init; }

    public required User User { get; init; }
}
