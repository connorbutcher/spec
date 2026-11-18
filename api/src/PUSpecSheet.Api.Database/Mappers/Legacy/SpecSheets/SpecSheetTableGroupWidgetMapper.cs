using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Legacy.Values;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.SpecSheets;

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
