using PUSpecSheet.Abstractions.Domain.Values;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.Database.Mappers.Values;

public static class LinkedSpecSheetValueMapper
{
    public static LinkedSpecSheetValue MapToDomain(this DbLinkedSpecSheetValue dbLinkedSpecSheetValue)
    {
        return new LinkedSpecSheetValue
        {
            Id = dbLinkedSpecSheetValue.Id,
            SpecSheetTableCellId = dbLinkedSpecSheetValue.SpecSheetTableCellId,
            TableReferenceId = dbLinkedSpecSheetValue.TableReferenceId,
            LinkedSpecSheetTypeId = dbLinkedSpecSheetValue.LinkedSpecSheetTypeId,
            LinkedSpecSheetPhaseId = dbLinkedSpecSheetValue.LinkedSpecSheetPhaseId,
            LinkedReferenceId = dbLinkedSpecSheetValue.LinkedReferenceId
        };
    }

    public static IQueryable<LinkedSpecSheetValue> MapToDomain(this IQueryable<DbLinkedSpecSheetValue> dbLinkedSpecSheetValues)
    {
        return dbLinkedSpecSheetValues
            .Select(db => db.MapToDomain());
    }
}
