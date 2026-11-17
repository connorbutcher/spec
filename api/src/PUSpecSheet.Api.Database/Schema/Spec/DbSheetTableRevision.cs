using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A revision of a sheet table: a draft (<see cref="Status"/> is draft) or what it was from <see cref="PublishedDateTime"/> until <see cref="SupersededDateTime"/>.
/// </summary>
public record DbSheetTableRevision
{
    public required int Id { get; init; }

    public required int SheetTableId { get; init; }

    public required int SheetId { get; init; }

    public required int RevisionNumber { get; init; }

    public required RevisionStatus Status { get; set; }

    public required string Title { get; set; }

    public required int DisplayOrder { get; set; }

    public required ITableProperties? ConfigurationOverride { get; set; }

    public required ICollection<SheetWidgetValue>? WidgetValues { get; set; }

    public required ICollection<SheetTableNote>? Notes { get; set; }

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
}
