using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Answers questions about a table's sections as the current user sees them.</summary>
public sealed class LiveSectionQuery(PuSpecSheetDbContext db, ICurrentUser currentUser)
{
    /// <summary>
    /// The table's section copies that exist for the user: published and not removed, or drafted by them,
    /// and not inside a removed section. Other people's unpublished drafts don't count.
    /// </summary>
    public async Task<IReadOnlyList<LiveSection>> VisibleAsync(int tableId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var sections = await db.SheetSections
            .AsNoTracking()
            .Where(section => section.SheetTableId == tableId)
            .Select(section => new { section.Id, section.TemplateSectionId, section.ParentSheetSectionId })
            .ToListAsync(cancellationToken);

        var revisions = await db.SheetSectionRevisions
            .Where(revision => revision.SheetSection.SheetTableId == tableId)
            .VisibleTo(me)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var resolved = RevisionResolver.Resolve(revisions, revision => revision.SheetSectionId, me);

        var own = sections
            .Where(section => RevisionResolver.IsVisible(resolved.GetValueOrDefault(section.Id)))
            .ToDictionary(section => section.Id);

        var visibleIds = own.Keys.ToHashSet();
        var parentOf = sections.ToDictionary(section => section.Id, section => section.ParentSheetSectionId);
        var result = new List<LiveSection>();
        foreach (var section in own.Values)
        {
            if (AncestorsVisible(section.ParentSheetSectionId, visibleIds, parentOf))
            {
                result.Add(new LiveSection(
                    section.Id,
                    section.TemplateSectionId,
                    section.ParentSheetSectionId,
                    resolved[section.Id].Shown!.DisplayOrder));
            }
        }

        return result;
    }

    /// <summary>The ids of a section and everything inside it, whether or not anyone can currently see them.</summary>
    public async Task<IReadOnlyCollection<int>> SubtreeIdsAsync(int tableId, int rootSectionId, CancellationToken cancellationToken)
    {
        var parents = await db.SheetSections
            .AsNoTracking()
            .Where(section => section.SheetTableId == tableId)
            .Select(section => new { section.Id, section.ParentSheetSectionId })
            .ToListAsync(cancellationToken);

        var ids = new HashSet<int> { rootSectionId };
        var added = true;
        while (added)
        {
            added = false;
            foreach (var section in parents)
            {
                if (section.ParentSheetSectionId is { } parent && ids.Contains(parent) && ids.Add(section.Id))
                {
                    added = true;
                }
            }
        }

        return ids;
    }

    private static bool AncestorsVisible(int? parentId, HashSet<int> visibleIds, Dictionary<int, int?> parentOf)
    {
        while (parentId is { } id)
        {
            if (!visibleIds.Contains(id))
            {
                return false;
            }

            parentId = parentOf.GetValueOrDefault(id);
        }

        return true;
    }
}
