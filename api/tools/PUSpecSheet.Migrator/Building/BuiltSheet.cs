using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Migrator.Building;

/// <summary>A legacy sheet rebuilt as identity rows and revisions, ready to save, with what to check once it is.</summary>
internal sealed record BuiltSheet
{
    public required DbSheet Sheet { get; init; }

    public required IReadOnlyList<BuiltVersion> Checks { get; init; }

    public required IReadOnlyList<string> Warnings { get; init; }
}
