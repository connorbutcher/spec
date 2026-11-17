namespace PUSpecSheet.Abstractions.Domain.Types;

public record SpecSheetTypeProperty
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

    public required bool SaveValueAsProperty { get; set; }

    public required int DisplayOrder { get; set; }

    public required ICollection<SpecSheetTypePropertyValidator>? Validators { get; set; } = [];
}
