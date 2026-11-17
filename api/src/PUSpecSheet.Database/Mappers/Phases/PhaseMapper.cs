using PUSpecSheet.Abstractions.Phases;
using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.Database.Mappers.Phases;

/// <summary>Phases as models.</summary>
public static class PhaseMapper
{
    /// <summary>Expects <see cref="DbPhase.SheetTypes"/> to be loaded.</summary>
    public static Phase ToModel(this DbPhase phase)
    {
        return new Phase
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
