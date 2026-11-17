using PUSpecSheet.Abstractions.Domain.Access;

namespace PUSpecSheet.Api.Emails.Models;

public record SpecSheetEmailArgs
{
    public required int SpecSheetId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required string TypeName { get; init; }

    public required string PhaseName { get; init; }

    public required string Content { get; init; }

    public required User User { get; init; }

    public string? RejectedBy { get; init; }
}
