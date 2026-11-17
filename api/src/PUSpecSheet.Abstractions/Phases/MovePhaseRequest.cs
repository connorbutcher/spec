using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Phases;

/// <summary>
/// Moves a phase to a 1-based <see cref="Position"/> among the phases under <see cref="ParentPhaseId"/>,
/// or among the top-level phases when that is null. Positions past the end move it last.
/// </summary>
public sealed record MovePhaseRequest
{
    public int? ParentPhaseId { get; init; }

    [Range(1, int.MaxValue)]
    public required int Position { get; init; }
}
