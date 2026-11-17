namespace PUSpecSheet.Api.Database.Schema.Types;

public record DbWidgetType
{
    public required int Id { get; init; }

    public required Guid Key { get; set; }

    public required string Title { get; set; }

    public required string Description { get; init; }
}
