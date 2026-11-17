namespace PUSpecSheet.Abstractions.Phases;

/// <summary>
/// A phase as a flat record. The UI builds the tree from <see cref="ParentPhaseId"/>.
/// </summary>
public sealed record PhaseDto
{
    public required int Id { get; init; }

    public required string Code { get; init; }

    public string? Description { get; init; }

    public required int DisplayOrder { get; init; }

    public int? ParentPhaseId { get; init; }

    public required IReadOnlyList<int> SheetTypeIds { get; init; }
}
