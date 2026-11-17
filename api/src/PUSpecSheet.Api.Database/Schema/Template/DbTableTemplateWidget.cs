using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Schema.Template;

public record DbTableTemplateWidget
{
    public required int Id { get; init; }

    public required int TableTemplateId { get; init; }

    public required int WidgetTypeId { get; init; }

    public virtual DbWidgetType? WidgetType { get; init; }
}
