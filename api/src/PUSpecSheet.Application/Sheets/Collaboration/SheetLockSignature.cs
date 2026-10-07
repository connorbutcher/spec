using System.Globalization;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Everything about a live sheet that other people can see change: its latest version and who each item
/// is checked out to. Two views of the same sheet give the same signature whoever is looking, so comparing
/// one with the last tells whether anyone else needs to refresh. Items that have never been published are
/// left out, as only their author can see them.
/// </summary>
public static class SheetLockSignature
{
    public static string Of(SheetDto sheet)
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
        return string.Create(CultureInfo.InvariantCulture, $"v{sheet.LatestVersionNumber ?? 0}|{string.Join(',', locks)}");
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
