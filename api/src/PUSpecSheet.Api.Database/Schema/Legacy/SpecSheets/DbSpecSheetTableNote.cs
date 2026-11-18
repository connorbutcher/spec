using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

public record DbSpecSheetTableNote
{
    public required int Id { get; init; }

    public required int SpecSheetTableId { get; init; }

    public required string Note { get; init; }

    public required string SourceSystem { get; init; }

    public required bool IsObsolete { get; set; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbUser? CreatedByUser { get; init; }
}
