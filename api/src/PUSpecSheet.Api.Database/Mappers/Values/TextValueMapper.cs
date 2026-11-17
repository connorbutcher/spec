using PUSpecSheet.Abstractions.Domain.Values;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.Database.Mappers.Values;

public static class TextValueMapper
{
    public static TextValue MapToDomain(this DbTextValue dbTextValue)
    {
        return new TextValue
        {
            Id = dbTextValue.Id,
            SpecSheetTableCellId = dbTextValue.SpecSheetTableCellId,
            Value = dbTextValue.Value
        };
    }

    public static IQueryable<TextValue> MapToDomain(this IQueryable<DbTextValue> dbTextValues)
    {
        return dbTextValues
            .Select(db => db.MapToDomain());
    }
}
