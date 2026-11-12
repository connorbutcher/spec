using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Phases;

/// <summary>
/// Adds a phase after its siblings: under <see cref="ParentPhaseId"/>, or at the top level when that is
/// null. <see cref="SheetTypeIds"/> are the sheet types the phase has; none when left out.
/// </summary>
public sealed record CreatePhaseRequest
{
    public CreatePhaseRequest(
        [Required, MaxLength(50)] string code,
        [MaxLength(500)] string? description,
        int? parentPhaseId,
        IReadOnlyList<int>? sheetTypeIds)
    {
        this.Code = code;
        this.Description = description;
        this.ParentPhaseId = parentPhaseId;
        this.SheetTypeIds = sheetTypeIds;
    }

    public string Code { get; init; }

    public string? Description { get; init; }

    public int? ParentPhaseId { get; init; }

    public IReadOnlyList<int>? SheetTypeIds { get; init; }
}
