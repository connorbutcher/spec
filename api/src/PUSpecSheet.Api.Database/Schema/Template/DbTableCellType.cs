using PUSpecSheet.Abstractions.Domain.Styling;

namespace PUSpecSheet.Api.Database.Schema.Template;

public record DbTableCellType
{
    public required int Id { get; init; }

    public required Guid Key { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Type { get; init; }

    public required string ModelType { get; init; }

    public required bool RequiresUserConfiguration { get; init; }

    public required CellStyling Styling { get; init; }
}
