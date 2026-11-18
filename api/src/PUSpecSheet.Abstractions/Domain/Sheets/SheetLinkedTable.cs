using PUSpecSheet.Abstractions.Domain.Access;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

public record SheetLinkedTable
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required Guid TableReferenceId { get; init; }

    public required int LinkedByUserId { get; init; }

    public User? LinkedByUser { get; init; }

    public required DateTime LinkedDateTime { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required SheetItemRevision Revision { get; init; }

    public required int DisplayOrder { get; init; }
}
