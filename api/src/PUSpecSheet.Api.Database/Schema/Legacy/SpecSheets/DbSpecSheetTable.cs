using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

public record DbSpecSheetTable
{
    public required int Id { get; init; }

    public required int SpecSheetId { get; init; }

    public required int TableTemplateId { get; init; }

    public required string Title { get; set; }

    public required int DisplayOrder { get; set; }

    public required Guid ReferenceId { get; init; }

    public required bool IsDeleted { get; set; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbUser? CreatedByUser { get; init; }

    public int? LastModifiedByUserId { get; set; }

    public DateTime? LastModifiedDateTime { get; set; }

    public ITableProperties? ConfigurationOverride { get; set; }

    public virtual DbUser? LastModifiedByUser { get; init; }

    public virtual DbTableTemplate? TableTemplate { get; set; }

    public virtual ICollection<DbSpecSheetTableGroup> Groups { get; set; } = [];

    public virtual ICollection<DbSpecSheetTableWidget> Widgets { get; set; } = [];

    public virtual ICollection<DbSpecSheetTableNote> Notes { get; set; } = [];
}
