using System.Globalization;
using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration;

// Everything about a live sheet that other people can see change: its latest version and who each item is checked out
// to. Two views of the same sheet give the same signature whoever is looking, so comparing one with the last tells
// whether anyone else needs to refresh. Items that have never been published are left out, as only their author can
// see them. The signature is a set of entries: v and the latest version, then one per locked item, its kind (T, B, S
// or R for a table, column block, section or row), id and holder, such as R12:3. Two signatures' differing entries
// say what changed.
public static class SheetLockSignature
{
    public static string Of(Sheet sheet)
    {
        return string.Join(',', Entries(sheet));
    }

    public static IReadOnlyList<string> Entries(Sheet sheet)
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

    // The rows whose locks differ between two signatures, or null when anything else differs too (the version, or a
    // table, column block or section), so only a whole read will do.
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

    private static void AddSections(List<string> locks, IReadOnlyList<SheetSection> sections)
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

    private static void Add(List<string> locks, char kind, int id, SheetLock? held, bool isPending)
    {
        if (held is not null && !isPending)
        {
            locks.Add(string.Create(CultureInfo.InvariantCulture, $"{kind}{id}:{held.UserId}"));
        }
    }
}
