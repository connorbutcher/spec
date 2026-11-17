namespace PUSpecSheet.Abstractions.Phases;

/// <summary>
/// A phase as a flat record. The UI builds the tree from <see cref="ParentPhaseId"/>.
/// </summary>
public sealed record PhaseDto
{
    public PhaseDto(int id, string code, string? description, int displayOrder, int? parentPhaseId, IReadOnlyList<int> sheetTypeIds)
    {
        Id = id;
        Code = code;
        Description = description;
        DisplayOrder = displayOrder;
        ParentPhaseId = parentPhaseId;
        SheetTypeIds = sheetTypeIds;
    }

    public int Id { get; init; }

    public string Code { get; init; }

    public string? Description { get; init; }

    public int DisplayOrder { get; init; }

    public int? ParentPhaseId { get; init; }

    public IReadOnlyList<int> SheetTypeIds { get; init; }
}
