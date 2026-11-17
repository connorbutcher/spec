using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Common;

/// <summary>Moves an item to a 1-based position among its siblings. Positions past the end move it last.</summary>
public sealed record MoveRequest
{
    [Range(1, int.MaxValue)]
    public required int DisplayOrder { get; init; }
}
