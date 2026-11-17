using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Abstractions.Domain.Template;

public record TableTemplateWidget
{
    public required int Id { get; init; }

    public required int TableTemplateId { get; init; }

    public required int WidgetTypeId { get; init; }

    public required WidgetType WidgetType { get; init; }
}
