using PUSpecSheet.Abstractions.Domain.Legacy.Values;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.Values;

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
