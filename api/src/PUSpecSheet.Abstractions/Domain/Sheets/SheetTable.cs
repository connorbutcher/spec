using PUSpecSheet.Abstractions.Domain.Properties.Table;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

public record SheetTable
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required int TableTemplateId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required SheetItemRevision Revision { get; init; }

    public required string Title { get; init; }

    public required int DisplayOrder { get; init; }

    public ITableProperties? ConfigurationOverride { get; init; }

    public required ICollection<SheetTableGroup> Groups { get; init; } = [];

    public required ICollection<SheetTableWidget> Widgets { get; init; } = [];

    public required ICollection<SheetTableNote> Notes { get; init; } = [];
}
