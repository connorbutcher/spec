namespace PUSpecSheet.Api.Database.Schema.Access;

public record DbSpecSheetPhaseObserver
{
    public required int Id { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required int UserId { get; init; }

    public required bool IsApprover { get; set; }

    public virtual DbUser? User { get; init; }
}
