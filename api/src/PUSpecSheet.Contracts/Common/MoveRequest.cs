using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Common;

/// <summary>Moves an item to a 1-based position among its siblings. Positions past the end move it last.</summary>
public sealed record MoveRequest([Range(1, int.MaxValue)] int DisplayOrder);
