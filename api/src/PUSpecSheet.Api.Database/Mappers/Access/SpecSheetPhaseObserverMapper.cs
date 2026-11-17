using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Mappers.Access;

public static class SpecSheetPhaseObserverMapper
{
    public static SpecSheetPhaseObserver MapToDomain(this DbSpecSheetPhaseObserver phaseObserver)
    {
        return new SpecSheetPhaseObserver
        {
            Id = phaseObserver.Id,
            SpecSheetPhaseId = phaseObserver.SpecSheetPhaseId,
            UserId = phaseObserver.UserId,
            IsApprover = phaseObserver.IsApprover,
            User = phaseObserver.User!.MapToDomain()
        };
    }

    public static IEnumerable<SpecSheetPhaseObserver> MapToDomain(this ICollection<DbSpecSheetPhaseObserver> phaseObservers)
    {
        return phaseObservers
            .Select(MapToDomain);
    }

    public static IQueryable<SpecSheetPhaseObserver> MapToDomain(this IQueryable<DbSpecSheetPhaseObserver> phaseObservers)
    {
        return phaseObservers
            .Select(phaseObserver => phaseObserver.MapToDomain());
    }
}
