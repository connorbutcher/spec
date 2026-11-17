using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.SpecSheets;

public record DbSpecSheetLinkedTable
{
    public required int Id { get; init; }

    public required int SpecSheetId { get; init; }

    public required int SpecSheetTableId { get; init; }

    public required int DisplayOrder { get; set; }

    public required Guid TableReferenceId { get; init; }

    public required int LinkedByUserId { get; init; }

    public required DateTime LinkedDateTime { get; init; }

    public virtual DbUser? LinkedByUser { get; init; }
}
