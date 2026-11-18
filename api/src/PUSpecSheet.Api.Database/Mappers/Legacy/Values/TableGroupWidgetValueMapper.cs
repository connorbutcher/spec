using PUSpecSheet.Abstractions.Domain.Legacy.Values;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.Values;

public static class TableGroupWidgetValueMapper
{
    public static TableGroupWidgetValue MapToDomain(this DbTableGroupWidgetValue dbTableGroupWidgetValue)
    {
        return new TableGroupWidgetValue
        {
            Id = dbTableGroupWidgetValue.Id,
            SpecSheetTableGroupWidgetId = dbTableGroupWidgetValue.SpecSheetTableGroupWidgetId,
            Value = dbTableGroupWidgetValue.Value
        };
    }

    public static IQueryable<TableGroupWidgetValue> MapToDomain(this IQueryable<DbTableGroupWidgetValue> dbTableGroupWidgetValues)
    {
        return dbTableGroupWidgetValues
            .Select(db => db.MapToDomain());
    }
}
