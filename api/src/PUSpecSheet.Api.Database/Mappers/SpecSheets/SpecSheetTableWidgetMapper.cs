using PUSpecSheet.Abstractions.Domain.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Values;
using PUSpecSheet.Api.Database.Schema.SpecSheets;

namespace PUSpecSheet.Api.Database.Mappers.SpecSheets;

public static class SpecSheetTableWidgetMapper
{
    public static SpecSheetTableWidget MapToDomain(this DbSpecSheetTableWidget dbSpecSheetTableWidget)
    {

        return new SpecSheetTableWidget
        {
            Id = dbSpecSheetTableWidget.Id,
            SpecSheetTableId = dbSpecSheetTableWidget.SpecSheetTableId,
            TableTemplateWidgetId = dbSpecSheetTableWidget.TableTemplateWidgetId,
            Value = dbSpecSheetTableWidget.Value!.MapToDomain()
        };
    }

    public static IEnumerable<SpecSheetTableWidget> MapToDomain(this ICollection<DbSpecSheetTableWidget> dbSpecSheetTableWidgets)
    {
        return dbSpecSheetTableWidgets.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetTableWidget> MapToDomain(this IQueryable<DbSpecSheetTableWidget> dbSpecSheetTableWidgets)
    {
        return dbSpecSheetTableWidgets
            .Select(db => db.MapToDomain());
    }
}
