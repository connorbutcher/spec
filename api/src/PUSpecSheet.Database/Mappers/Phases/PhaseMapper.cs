using PUSpecSheet.Abstractions.Phases;
using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.Database.Mappers.Phases;

/// <summary>Phases as DTOs.</summary>
public static class PhaseMapper
{
    /// <summary>Expects <see cref="DbPhase.SheetTypes"/> to be loaded.</summary>
    public static PhaseDto ToDto(this DbPhase phase)
    {
        return new PhaseDto
        {
            Id = phase.Id,
            Code = phase.Code,
            Description = phase.Description,
            DisplayOrder = phase.DisplayOrder,
            ParentPhaseId = phase.ParentPhaseId,
            SheetTypeIds = phase.SheetTypes
                .Select(link => link.SheetTypeId)
                .Order()
                .ToList(),
        };
    }
}
