namespace PUSpecSheet.Abstractions.Domain.References;

public sealed record SpecSheetExternalReference
{
    public required int Id { get; init; }

    public required int SpecSheetId { get; init; }

    public required int ExternalId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required string SystemName { get; init; }

    public required string OwnerName { get; init; }

    public required string Url { get; init; }

    public required string Type { get; init; }

    public string? Grouping { get; init; }

    public string? SystemIconUrl { get; init; }

    public string? Description { get; init; }
}
