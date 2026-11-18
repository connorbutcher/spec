namespace PUSpecSheet.Api.Database.Schema.Legacy.References;

public record DbSpecSheetExternalReference
{
    public required int Id { get; init; }

    public required int SpecSheetId { get; init; }

    public required int ExternalId { get; set; }

    public required Guid ReferenceId { get; init; }

    public required string SystemName { get; set; }

    public required string OwnerName { get; set; }

    public required string Url { get; set; }

    public required string Type { get; set; }

    public string? Grouping { get; set; }

    public string? SystemIconUrl { get; set; }

    public string? Description { get; set; }
}
