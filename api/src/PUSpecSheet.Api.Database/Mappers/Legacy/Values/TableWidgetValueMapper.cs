using PUSpecSheet.Abstractions.Domain.Legacy.Values;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.Values;

public static class TableWidgetValueMapper
{
    public static TableWidgetValue MapToDomain(this DbTableWidgetValue dbTableWidgetValue)
    {
        return new TableWidgetValue
        {
            Id = dbTableWidgetValue.Id,
            SpecSheetTableWidgetId = dbTableWidgetValue.SpecSheetTableWidgetId,
            Value = dbTableWidgetValue.Value
        };
    }

    public static IQueryable<TableWidgetValue> MapToDomain(this IQueryable<DbTableWidgetValue> dbTableWidgetValues)
    {
        return dbTableWidgetValues
            .Select(db => db.MapToDomain());
    }
}
