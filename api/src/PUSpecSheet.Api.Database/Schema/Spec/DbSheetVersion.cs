using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A published version of a sheet. <see cref="SourceSpecSheetId"/> is the SpecSheets copy it was migrated from.
/// </summary>
public record DbSheetVersion
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required int VersionNumber { get; init; }

    public required DateTime PublishedDateTime { get; init; }

    public required int PublishedByUserId { get; init; }

    public required string? ChangeLog { get; set; }

    public required int? SourceSpecSheetId { get; init; }

    public required int? LegacySpecSheetId { get; init; }

    public required bool HasBeenMigrated { get; init; }

    public virtual DbUser? PublishedByUser { get; init; }
}
