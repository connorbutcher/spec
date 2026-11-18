using PUSpecSheet.Abstractions.Domain.Legacy.Values;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.Values;

public static class PairValueMapper
{
    public static PairValue MapToDomain(this DbPairValue dbPairValue)
    {
        return new PairValue
        {
            Id = dbPairValue.Id,
            SpecSheetTableCellId = dbPairValue.SpecSheetTableCellId,
            ValueX = dbPairValue.PairValueX,
            ValueY = dbPairValue.PairValueY,
        };
    }

    public static IQueryable<PairValue> MapToDomain(this IQueryable<DbPairValue> dbPairValues)
    {
        return dbPairValues
            .Select(db => db.MapToDomain());
    }
}
