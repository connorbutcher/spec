namespace PUSpecSheet.Abstractions.Domain.Configuration.ValidatorProperties;

public sealed record MinMaxNumberValidatorProperty : IValidatorProperty
{
    public required double Number { get; init; }
}
