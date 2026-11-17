using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Api.Database.Schema.Types;

public record DbSpecSheetTypeProperty
{
    public required int Id { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required Guid ReferenceId { get; set; }

    public required string ValueName { get; set; }

    public required string ValueKey { get; set; }

    public required string ValueType { get; set; }

    public required string? PlaceholderText { get; set; }

    public required string ValueControlType { get; set; }

    public required bool CanValueBeSetInForm { get; set; }

    public required int DisplayOrder { get; set; }

    public required bool SaveValueAsProperty { get; set; }

    public required ICollection<SpecSheetTypePropertyValidator>? ValidatorConfiguration { get; set; } = [];
}
