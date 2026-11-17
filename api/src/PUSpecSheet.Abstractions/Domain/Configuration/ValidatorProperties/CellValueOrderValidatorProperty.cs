namespace PUSpecSheet.Abstractions.Domain.Configuration.ValidatorProperties;

public sealed record CellValueOrderValidatorProperty : IValidatorProperty
{
    public required ICollection<Guid> CellKeys { get; init; }

    /// <summary>
    /// Get the order in which the cell values should appear, e.g., "asc" for ascending or "desc" for descending.
    /// </summary>
    public required string Order { get; init; }
}
