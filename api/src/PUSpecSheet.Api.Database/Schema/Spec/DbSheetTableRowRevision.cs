using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Cell;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A revision of a sheet table row: a draft (<see cref="Status"/> is draft) or what it was from <see cref="PublishedDateTime"/> until <see cref="SupersededDateTime"/>. Its cells' values belong to it.
/// </summary>
public record DbSheetTableRowRevision : IDbSheetRevision
{
    public required int Id { get; init; }

    public required int SheetTableRowId { get; init; }

    public required int SheetId { get; init; }

    public required int RevisionNumber { get; init; }

    public required RevisionStatus Status { get; set; }

    public required int DisplayOrder { get; set; }

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

    public virtual ICollection<DbSheetTextValue> TextValues { get; init; } = [];

    public virtual ICollection<DbSheetNumericValue> NumericValues { get; init; } = [];

    public virtual ICollection<DbSheetPairValue> PairValues { get; init; } = [];

    public virtual ICollection<DbSheetLinkedSheetValue> LinkedSheetValues { get; init; } = [];

    public virtual ICollection<DbSheetCellConfigurationOverride> CellConfigurationOverrides { get; init; } = [];

    public virtual DbSheetVersion? SheetVersion { get; set; }
}
