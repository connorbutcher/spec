namespace PUSpecSheet.Query.Models.Phase;

public record SlimSpecSheetPhaseWithChildren : SlimPUSpecSheetPhase
{
    public required ICollection<SlimSpecSheetPhaseWithChildren> Phases { get; init; } = [];

    public required ICollection<SlimPUSpecSheetType> TypesSupportedForPhase { get; init; } = [];
}
