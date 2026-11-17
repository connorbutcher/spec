using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Mappers.Template;

public static class TableTemplateWidgetMapper
{
    public static TableTemplateWidget MapToDomain(this DbTableTemplateWidget dbTableTemplateWidget)
    {
        return new TableTemplateWidget
        {
            Id = dbTableTemplateWidget.Id,
            TableTemplateId = dbTableTemplateWidget.TableTemplateId,
            WidgetTypeId = dbTableTemplateWidget.WidgetTypeId,
            WidgetType = dbTableTemplateWidget.WidgetType!.MapToDomain()
        };
    }

    public static IEnumerable<TableTemplateWidget> MapToDomain(this ICollection<DbTableTemplateWidget> dbTableTemplateWidgets)
    {
        return dbTableTemplateWidgets.Select(MapToDomain);
    }

    public static IQueryable<TableTemplateWidget> MapToDomain(this IQueryable<DbTableTemplateWidget> dbTableTemplateWidgets)
    {
        return dbTableTemplateWidgets
            .Select(dbTableTemplateWidget => dbTableTemplateWidget.MapToDomain());
    }
}
