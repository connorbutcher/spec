using PUSpecSheet.Contracts.Phases;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Phases;

internal static class PhaseMappings
{
    /// <summary>Maps a phase to its DTO. Expects <see cref="DbPhase.SheetTypes"/> to be loaded.</summary>
    public static PhaseDto ToDto(this DbPhase phase)
    {
        var sheetTypeIds = phase.SheetTypes
            .Select(link => link.SheetTypeId)
            .Order()
            .ToList();

        return new PhaseDto(
            phase.Id,
            phase.Code,
            phase.Description,
            phase.DisplayOrder,
            phase.ParentPhaseId,
            sheetTypeIds);
    }
}
