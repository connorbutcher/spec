using PUSpecSheet.Abstractions.Domain.Configuration.ValidatorProperties;
using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Abstractions.Domain.Configuration;

public sealed record Validator
{
    public required string Type { get; init; }

    public required string Message { get; init; }

    public required ValidatorLevel Level { get; init; }

    public IValidatorProperty? Property { get; init; }
}
