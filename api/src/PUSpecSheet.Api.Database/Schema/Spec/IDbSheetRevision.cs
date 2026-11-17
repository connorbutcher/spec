using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// What the revisions of a sheet's tables, groups, rows, properties and linked tables have in common. A revision is a
/// draft until published, and the published revisions of one item follow one another: each is what the item was from
/// its <see cref="PublishedDateTime"/> until its <see cref="SupersededDateTime"/>. External reference revisions are
/// only ever published, so they aren't one of these.
/// </summary>
public interface IDbSheetRevision
{
    int Id { get; }

    int SheetId { get; }

    int RevisionNumber { get; }

    RevisionStatus Status { get; set; }

    bool IsDeleted { get; set; }

    int CreatedByUserId { get; }

    DateTime CreatedDateTime { get; }

    DateTime LastModifiedDateTime { get; set; }

    DateTime? PublishedDateTime { get; set; }

    DateTime? SupersededDateTime { get; set; }

    int? SheetVersionId { get; set; }

    int? SheetSubmissionId { get; set; }

    DbSheetVersion? SheetVersion { get; set; }
}
