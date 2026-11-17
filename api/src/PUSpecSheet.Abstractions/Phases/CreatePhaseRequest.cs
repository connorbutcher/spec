using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Phases;

/// <summary>
/// Adds a phase after its siblings: under <see cref="ParentPhaseId"/>, or at the top level when that is
/// null. <see cref="SheetTypeIds"/> are the sheet types the phase has; none when left out.
/// </summary>
public sealed record CreatePhaseRequest
{
    [Required, MaxLength(50)]
    public required string Code { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    public int? ParentPhaseId { get; init; }

    public IReadOnlyList<int>? SheetTypeIds { get; init; }
}
