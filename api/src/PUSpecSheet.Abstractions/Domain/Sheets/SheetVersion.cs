using PUSpecSheet.Abstractions.Domain.Access;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

public record SheetVersion
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required int VersionNumber { get; init; }

    public required DateTime PublishedDateTime { get; init; }

    public required int PublishedByUserId { get; init; }

    public User? PublishedByUser { get; init; }

    public required string? ChangeLog { get; init; }

    public required bool HasBeenMigrated { get; init; }
}
