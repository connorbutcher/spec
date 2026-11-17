using System.Globalization;

namespace PUSpecSheet.Migrator;

/// <summary>
/// What the migrator was asked to do, from its command line.
/// </summary>
internal sealed record MigratorOptions
{
    public const string Usage = """
        Brings each sheet's revisions (the spec, cell and approval tables) into step with its copies (SpecSheets and
        the tables under it): a sheet whose copies changed since it was last migrated is built again from them.

          --connection <connection string>   the PUSpecSheet database (required)
          --phase <phase id>                 only this phase's sheets; may be given more than once
          --rebuild                          build every sheet again, changed or not
          --commit                           keep what is built; without it every sheet is built, checked and
                                             rolled back (a dry run)
        """;

    public required string ConnectionString { get; init; }

    /// <summary>
    /// The phases whose sheets to migrate; every phase's when empty.
    /// </summary>
    public required IReadOnlySet<int> PhaseIds { get; init; }

    public required bool Rebuild { get; init; }

    public required bool Commit { get; init; }

    /// <summary>
    /// The options, or null when the arguments don't make sense.
    /// </summary>
    public static MigratorOptions? Parse(IReadOnlyList<string> args)
    {
        string? connectionString = null;
        var phaseIds = new HashSet<int>();
        var rebuild = false;
        var commit = false;

        for (var index = 0; index < args.Count; index++)
        {
            var value = index + 1 < args.Count ? args[index + 1] : null;
            switch (args[index])
            {
                case "--connection" when value is not null:
                    connectionString = value;
                    index++;
                    break;
                case "--phase" when int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var phaseId):
                    phaseIds.Add(phaseId);
                    index++;
                    break;
                case "--rebuild":
                    rebuild = true;
                    break;
                case "--commit":
                    commit = true;
                    break;
                default:
                    return null;
            }
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        return new MigratorOptions
        {
            ConnectionString = connectionString,
            PhaseIds = phaseIds,
            Rebuild = rebuild,
            Commit = commit
        };
    }
}
