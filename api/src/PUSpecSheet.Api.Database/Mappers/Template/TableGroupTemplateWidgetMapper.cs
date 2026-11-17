using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Mappers.Template;

public static class TableGroupTemplateWidgetMapper
{
    public static TableGroupTemplateWidget MapToDomain(this DbTableGroupTemplateWidget dbTableGroupTemplateWidget)
    {
        return new TableGroupTemplateWidget
        {
            Id = dbTableGroupTemplateWidget.Id,
            TableGroupTemplateId = dbTableGroupTemplateWidget.TableGroupTemplateId,
            WidgetTypeId = dbTableGroupTemplateWidget.WidgetTypeId,
            WidgetType = dbTableGroupTemplateWidget.WidgetType!.MapToDomain()
        };
    }

    public static IEnumerable<TableGroupTemplateWidget> MapToDomain(this ICollection<DbTableGroupTemplateWidget> dbTableGroupTemplateWidgets)
    {
        return dbTableGroupTemplateWidgets.Select(MapToDomain);
    }

    public static IQueryable<TableGroupTemplateWidget> MapToDomain(this IQueryable<DbTableGroupTemplateWidget> dbTableGroupTemplateWidgets)
    {
        return dbTableGroupTemplateWidgets
            .Select(dbTableGroupTemplateWidget => dbTableGroupTemplateWidget.MapToDomain());
    }
}
