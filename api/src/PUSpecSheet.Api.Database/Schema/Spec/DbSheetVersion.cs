using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A published version of a sheet. A version migrated from a SpecSheets copy keeps the copy's id, creator and creation time in the Source columns.
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

    public required int? SourceCreatedByUserId { get; init; }

    public required DateTime? SourceCreatedDateTime { get; init; }

    public virtual DbUser? PublishedByUser { get; init; }
}
