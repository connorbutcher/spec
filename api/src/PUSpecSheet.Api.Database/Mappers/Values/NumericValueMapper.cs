using PUSpecSheet.Abstractions.Domain.Values;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.Database.Mappers.Values;

internal static class NumericValueMapper
{
    public static NumericValue MapToDomain(this DbNumericValue dbNumericValue)
    {
        return new NumericValue
        {
            Id = dbNumericValue.Id,
            SpecSheetTableCellId = dbNumericValue.SpecSheetTableCellId,
            Value = dbNumericValue.Value
        };
    }

    public static IQueryable<NumericValue> MapToDomain(this IQueryable<DbNumericValue> dbNumericValues)
    {
        return dbNumericValues
            .Select(db => db.MapToDomain());
    }
}
