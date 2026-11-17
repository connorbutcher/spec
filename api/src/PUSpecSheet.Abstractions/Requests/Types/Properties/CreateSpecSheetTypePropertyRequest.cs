using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Abstractions.Requests.Types.Properties;

public sealed record CreateSpecSheetTypePropertyRequest
{
    public required int SpecSheetTypeId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required string ValueName { get; init; }

    public required string ValueKey { get; init; }

    public required string ValueType { get; init; }

    public string? PlaceholderText { get; init; }

    public required string ValueControlType { get; init; }

    public required bool CanValueBeSetInForm { get; init; }

    public required int DisplayOrder { get; init; }

    public ICollection<SpecSheetTypePropertyValidator> Validators { get; init; } = [];
}
