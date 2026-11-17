namespace PUSpecSheet.Abstractions.Domain.Types;

public record WidgetType
{
    public required int Id { get; init; }

    public required Guid Key { get; set; }

    public required string Title { get; set; }

    public required string Description { get; init; }
}
