using PUSpecSheet.Abstractions.Domain.Configuration.ValidatorProperties;

namespace PUSpecSheet.Abstractions.Domain.Types;

public sealed record SpecSheetTypePropertyValidator
{
    public required string Validator { get; init; }

    public required string Message { get; init; }

    public IValidatorProperty? Property { get; init; }
}
