namespace PUSpecSheet.Abstractions.Domain.Configuration.ValidatorProperties;

public sealed record MinMaxLengthValidatorProperty : IValidatorProperty
{
    public required int Length { get; init; }
}
