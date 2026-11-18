using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Properties.Table;

namespace PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;

public record SpecSheetTable
{
    public required int Id { get; init; }

    public required int SpecSheetId { get; init; }

    public required int TableTemplateId { get; init; }

    public required string Title { get; init; }

    public required int DisplayOrder { get; init; }

    public required Guid ReferenceId { get; init; }

    public required bool IsDeleted { get; init; }

    public required ICollection<SpecSheetTableGroup> Groups { get; set; }

    public required ICollection<SpecSheetTableWidget> Widgets { get; set; }

    public required ICollection<SpecSheetTableNote> Notes { get; init; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual User? CreatedByUser { get; init; }

    public int? LastModifiedByUserId { get; init; }

    public DateTime? LastModifiedDateTime { get; init; }

    public virtual User? LastModifiedByUser { get; init; }

    public ITableProperties? ConfigurationOverride { get; init; }
}
