using System.Globalization;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Everything about a live sheet that other people can see change: its latest version and who each item
/// is checked out to. Two views of the same sheet give the same signature whoever is looking, so comparing
/// one with the last tells whether anyone else needs to refresh. Items that have never been published are
/// left out, as only their author can see them.
/// <para>
/// The signature is a set of entries: <c>v</c> and the latest version, then one per locked item, its kind
/// (T, B, S or R for a table, column block, section or row), id and holder, such as <c>R12:3</c>. Two
/// signatures' differing entries say what changed.
/// </para>
/// </summary>
public static class SheetLockSignature
{
    public static string Of(SheetDto sheet)
    {
        return string.Join(',', Entries(sheet));
    }

    /// <summary>The signature's entries, in order: the version first, then the locks.</summary>
    public static IReadOnlyList<string> Entries(SheetDto sheet)
    {
        var locks = new List<string>();
        foreach (var table in sheet.Tables)
        {
            Add(locks, 'T', table.Id, table.Lock, table.IsPending);
            foreach (var block in table.ColumnBlocks)
            {
                Add(locks, 'B', block.Id, block.Lock, block.IsPending);
            }

            AddSections(locks, table.Sections);
        }

        locks.Sort(StringComparer.Ordinal);
        locks.Insert(0, string.Create(CultureInfo.InvariantCulture, $"v{sheet.LatestVersionNumber ?? 0}"));
        return locks;
    }

    /// <summary>
    /// The rows whose locks differ between two signatures, or null when anything else differs too (the
    /// version, or a table, column block or section), so only a whole read will do.
    /// </summary>
    public static IReadOnlyList<int>? ChangedRows(IReadOnlyCollection<string> before, IReadOnlyCollection<string> after)
    {
        var differing = before.Except(after, StringComparer.Ordinal)
            .Concat(after.Except(before, StringComparer.Ordinal))
            .ToList();
        if (differing.Any(entry => entry[0] != 'R'))
        {
            return null;
        }

        return differing
            .Select(entry => int.Parse(entry.AsSpan(1, entry.IndexOf(':', StringComparison.Ordinal) - 1), CultureInfo.InvariantCulture))
            .Distinct()
            .Order()
            .ToList();
    }

    private static void AddSections(List<string> locks, IReadOnlyList<SheetSectionDto> sections)
    {
        foreach (var section in sections)
        {
            Add(locks, 'S', section.Id, section.Lock, section.IsPending);
            foreach (var row in section.Rows)
            {
                Add(locks, 'R', row.Id, row.Lock, row.IsPending);
            }

            AddSections(locks, section.Sections);
        }
    }

    private static void Add(List<string> locks, char kind, int id, SheetLockDto? held, bool isPending)
    {
        if (held is not null && !isPending)
        {
            locks.Add(string.Create(CultureInfo.InvariantCulture, $"{kind}{id}:{held.UserId}"));
        }
    }
}
