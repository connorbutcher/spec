namespace PUSpecSheet.Contracts.Phases;

/// <summary>
/// A phase as a flat record. The UI builds the tree from <see cref="ParentPhaseId"/>.
/// </summary>
public sealed record PhaseDto(
    int Id,
    string Code,
    string? Description,
    int DisplayOrder,
    int? ParentPhaseId,
    IReadOnlyList<int> SheetTypeIds);
