using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// Another sheet's table shown on this sheet, by the table's ReferenceID.
/// </summary>
public record DbSheetLinkedTable
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required Guid TableReferenceId { get; init; }

    public required int LinkedByUserId { get; init; }

    public required DateTime LinkedDateTime { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbUser? LinkedByUser { get; init; }

    public virtual ICollection<DbSheetLinkedTableRevision> Revisions { get; init; } = [];
}
