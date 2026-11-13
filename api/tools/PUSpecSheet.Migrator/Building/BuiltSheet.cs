using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Migrator.Building;

/// <summary>A legacy sheet rebuilt as identity rows and revisions, ready to save, with what to check once it is.</summary>
internal sealed record BuiltSheet(DbSheet Sheet, IReadOnlyList<BuiltVersion> Checks, IReadOnlyList<string> Warnings);
