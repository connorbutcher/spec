using PUSpecSheet.Abstractions.Domain.Access;

namespace PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;

public record SpecSheetTableNote
{
    public required int Id { get; init; }

    public required int SpecSheetTableId { get; init; }

    public required string Note { get; init; }

    public required string SourceSystem { get; init; }

    public required bool IsObsolete { get; init; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required User CreatedByUser { get; init; }
}
