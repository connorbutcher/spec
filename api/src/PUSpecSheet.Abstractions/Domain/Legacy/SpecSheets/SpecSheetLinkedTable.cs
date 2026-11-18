using PUSpecSheet.Abstractions.Domain.Access;

namespace PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;

public record SpecSheetLinkedTable
{
    public required int Id { get; init; }

    public required int SpecSheetId { get; init; }

    public required int DisplayOrder { get; set; }

    public required Guid TableReferenceId { get; init; }

    public required int LinkedByUserId { get; init; }

    public required DateTime LinkedDateTime { get; init; }

    public virtual User? LinkedByUser { get; init; }
}
