using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

public record DbSpecSheetTableGroup
{
    public required int Id { get; init; }

    public required int SpecSheetTableId { get; init; }

    public required int TableGroupTemplateId { get; init; }

    public required int? LinkedSpecSheetTableGroupId { get; set; }

    public required int Order { get; set; }

    public required Guid ReferenceId { get; init; }

    public required bool IsDeleted { get; set; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbUser? CreatedByUser { get; init; }

    public int? LastModifiedByUserId { get; set; }

    public DateTime? LastModifiedDateTime { get; set; }

    public virtual DbUser? LastModifiedByUser { get; init; }

    public virtual ICollection<DbSpecSheetTableRow> Rows { get; set; } = [];

    public virtual ICollection<DbSpecSheetTableGroupWidget> Widgets { get; set; } = [];

    public virtual DbTableGroupTemplate? TableGroupTemplate { get; set; }

    public virtual DbSpecSheetTableGroup? LinkedSpecSheetTableGroup { get; set; }
}
