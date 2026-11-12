using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Phases;

/// <summary>
/// Moves a phase to a 1-based <see cref="Position"/> among the phases under <see cref="ParentPhaseId"/>,
/// or among the top-level phases when that is null. Positions past the end move it last.
/// </summary>
public sealed record MovePhaseRequest
{
    public MovePhaseRequest(int? parentPhaseId, [Range(1, int.MaxValue)] int position)
    {
        this.ParentPhaseId = parentPhaseId;
        this.Position = position;
    }

    public int? ParentPhaseId { get; init; }

    public int Position { get; init; }
}
