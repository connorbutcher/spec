using System.Globalization;

namespace PUSpecSheet.Migrator;

/// <summary>What the migrator was asked to do, from its command line.</summary>
internal sealed record MigratorOptions
{
    public const string Usage = """
        Moves sheets from the legacy layout (each version a whole copy) onto the revision tables.

          --legacy <connection string>   the database holding the [legacy] tables (required)
          --target <connection string>   the database to migrate into; defaults to --legacy
          --phase <phase id>             migrate only this phase's sheets; may be given more than once
          --commit                       keep the migrated sheets; without it every sheet is migrated,
                                         checked and rolled back (a dry run)
        """;

    public string LegacyConnectionString { get; init; } = string.Empty;

    public string TargetConnectionString { get; init; } = string.Empty;

    /// <summary>The phases whose sheets to migrate; every phase's when empty.</summary>
    public IReadOnlySet<int> PhaseIds { get; init; } = new HashSet<int>();

    /// <summary>Whether to keep what was migrated. Without it each sheet is rolled back once checked.</summary>
    public bool Commit { get; init; }

    /// <summary>The options, or null when the arguments don't make sense.</summary>
    public static MigratorOptions? Parse(IReadOnlyList<string> args)
    {
        string? legacy = null;
        string? target = null;
        var phaseIds = new HashSet<int>();
        var commit = false;

        for (var index = 0; index < args.Count; index++)
        {
            var value = index + 1 < args.Count ? args[index + 1] : null;
            switch (args[index])
            {
                case "--legacy" when value is not null:
                    legacy = value;
                    index++;
                    break;
                case "--target" when value is not null:
                    target = value;
                    index++;
                    break;
                case "--phase" when int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var phaseId):
                    phaseIds.Add(phaseId);
                    index++;
                    break;
                case "--commit":
                    commit = true;
                    break;
                default:
                    return null;
            }
        }

        if (string.IsNullOrWhiteSpace(legacy))
        {
            return null;
        }

        return new MigratorOptions
        {
            LegacyConnectionString = legacy,
            TargetConnectionString = string.IsNullOrWhiteSpace(target) ? legacy : target,
            PhaseIds = phaseIds,
            Commit = commit,
        };
    }
}
