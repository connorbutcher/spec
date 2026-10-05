using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Phases;

/// <summary>
/// Adds a phase after its siblings: under <see cref="ParentPhaseId"/>, or at the top level when that is
/// null. <see cref="SheetTypeIds"/> are the sheet types the phase has; none when left out.
/// </summary>
public sealed record CreatePhaseRequest(
    [Required, MaxLength(50)] string Code,
    [MaxLength(500)] string? Description,
    int? ParentPhaseId,
    IReadOnlyList<int>? SheetTypeIds);
