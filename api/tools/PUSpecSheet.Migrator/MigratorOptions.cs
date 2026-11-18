using System.Text.Json;

namespace PUSpecSheet.Migrator;

/// <summary>
/// What the migrator was asked to do, from its JSON options file.
/// </summary>
internal sealed record MigratorOptions
{
    public const string DefaultPath = "migrator.json";

    public const string Usage = """
        Brings each sheet's revisions (the spec, cell and approval tables) into step with its copies (SpecSheets and
        the tables under it): a sheet whose copies changed since it was last migrated is built again from them.

          PUSpecSheet.Migrator [options file]    the file defaults to migrator.json in the current directory

        The options file is JSON (see migrator.example.json):

          connectionString   the PUSpecSheet database (required)
          phaseIds           only these phases' sheets; every phase's when empty or left out
          sheetTypeIds       only these sheet types' sheets; every type's when empty or left out
          rebuild            build every sheet again, changed or not (default false)
          commit             keep what is built; without it every sheet is built, checked and rolled back
                             (a dry run) (default false)
        """;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow
    };

    public required string ConnectionString { get; init; }

    /// <summary>
    /// The phases whose sheets to migrate; every phase's when empty.
    /// </summary>
    public required IReadOnlySet<int> PhaseIds { get; init; }

    /// <summary>
    /// The sheet types whose sheets to migrate; every type's when empty.
    /// </summary>
    public required IReadOnlySet<int> SheetTypeIds { get; init; }

    public required bool Rebuild { get; init; }

    public required bool Commit { get; init; }

    /// <summary>
    /// Whether the sheet is one to migrate: it is in one of the phases and one of the sheet types, where any are given.
    /// </summary>
    public bool Includes(int phaseId, int sheetTypeId)
    {
        return (PhaseIds.Count == 0 || PhaseIds.Contains(phaseId))
            && (SheetTypeIds.Count == 0 || SheetTypeIds.Contains(sheetTypeId));
    }

    /// <summary>
    /// Reads the options file; on failure the options are null and the error says why.
    /// </summary>
    public static bool TryLoad(string path, out MigratorOptions? options, out string? error)
    {
        options = null;
        error = null;

        if (!File.Exists(path))
        {
            error = $"The options file {Path.GetFullPath(path)} doesn't exist.";
            return false;
        }

        FileOptions? file;
        try
        {
            file = JsonSerializer.Deserialize<FileOptions>(File.ReadAllText(path), JsonOptions);
        }
        catch (JsonException exception)
        {
            error = $"The options file isn't right: {exception.Message}";
            return false;
        }

        if (file is null || string.IsNullOrWhiteSpace(file.ConnectionString))
        {
            error = "The options file has no connectionString.";
            return false;
        }

        options = new MigratorOptions
        {
            ConnectionString = file.ConnectionString,
            PhaseIds = file.PhaseIds.ToHashSet(),
            SheetTypeIds = file.SheetTypeIds.ToHashSet(),
            Rebuild = file.Rebuild,
            Commit = file.Commit
        };
        return true;
    }

    private sealed record FileOptions
    {
        public string? ConnectionString { get; init; }

        public List<int> PhaseIds { get; init; } = [];

        public List<int> SheetTypeIds { get; init; } = [];

        public bool Rebuild { get; init; }

        public bool Commit { get; init; }
    }
}
