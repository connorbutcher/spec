using PUSpecSheet.Abstractions.Domain.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Values;
using PUSpecSheet.Api.Database.Schema.SpecSheets;

namespace PUSpecSheet.Api.Database.Mappers.SpecSheets;

public static class SpecSheetTableGroupWidgetMapper
{
    public static SpecSheetTableGroupWidget MapToDomain(this DbSpecSheetTableGroupWidget dbSpecSheetTableGroupWidget)
    {

        return new SpecSheetTableGroupWidget
        {
            Id = dbSpecSheetTableGroupWidget.Id,
            SpecSheetTableGroupId = dbSpecSheetTableGroupWidget.SpecSheetTableGroupId,
            TableGroupTemplateWidgetId = dbSpecSheetTableGroupWidget.TableGroupTemplateWidgetId,
            Value = dbSpecSheetTableGroupWidget.Value!.MapToDomain()
        };
    }

    public static IEnumerable<SpecSheetTableGroupWidget> MapToDomain(this ICollection<DbSpecSheetTableGroupWidget> dbSpecSheetTableGroupWidgets)
    {
        return dbSpecSheetTableGroupWidgets.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetTableGroupWidget> MapToDomain(this IQueryable<DbSpecSheetTableGroupWidget> dbSpecSheetTableGroupWidgets)
    {
        return dbSpecSheetTableGroupWidgets
            .Select(db => db.MapToDomain());
    }
}
