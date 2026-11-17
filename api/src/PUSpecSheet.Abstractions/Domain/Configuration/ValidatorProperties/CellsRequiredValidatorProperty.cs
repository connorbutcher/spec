namespace PUSpecSheet.Abstractions.Domain.Configuration.ValidatorProperties;

public sealed record CellsRequiredValidatorProperty : IValidatorProperty
{
    public required ICollection<Guid> CellKeys { get; init; }
}
