using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Schema.Template;

public record DbTableGroupTemplateWidget
{
    public required int Id { get; init; }

    public required int TableGroupTemplateId { get; init; }

    public required int WidgetTypeId { get; init; }

    public virtual DbWidgetType? WidgetType { get; init; }
}
