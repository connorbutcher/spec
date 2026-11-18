using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A revision of a sheet table group: a draft (<see cref="Status"/> is draft) or what it was from <see cref="PublishedDateTime"/> until <see cref="SupersededDateTime"/>.
/// </summary>
public record DbSheetTableGroupRevision : IDbSheetRevision
{
    public required int Id { get; init; }

    public required int SheetTableGroupId { get; init; }

    public required int SheetId { get; init; }

    public required int RevisionNumber { get; init; }

    public required RevisionStatus Status { get; set; }

    public required int DisplayOrder { get; set; }

    public required Guid? LinkedSheetTableGroupReferenceId { get; set; }

    public required bool IsDeleted { get; set; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required DateTime LastModifiedDateTime { get; set; }

    public required DateTime? PublishedDateTime { get; set; }

    public required DateTime? SupersededDateTime { get; set; }

    public required int? SheetVersionId { get; set; }

    public required int? SheetSubmissionId { get; set; }

    public byte[] RowVersion { get; init; } = [];

    public virtual DbUser? CreatedByUser { get; init; }

    public virtual DbSheetVersion? SheetVersion { get; set; }

    public virtual ICollection<DbSheetTableGroupWidgetValue> WidgetValues { get; init; } = [];
}
