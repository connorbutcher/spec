using PUSpecSheet.Abstractions.Phases;
using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.Database.Mappers.Phases;

/// <summary>Phases as DTOs.</summary>
public static class PhaseMapper
{
    /// <summary>Expects <see cref="DbPhase.SheetTypes"/> to be loaded.</summary>
    public static PhaseDto ToDto(this DbPhase phase)
    {
        return new PhaseDto(
            phase.Id,
            phase.Code,
            phase.Description,
            phase.DisplayOrder,
            phase.ParentPhaseId,
            phase.SheetTypes
                .Select(link => link.SheetTypeId)
                .Order()
                .ToList());
    }
}
