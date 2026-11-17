using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Users;

namespace PUSpecSheet.DataAccess.Sheets.Layout;

public sealed class LiveSectionQuery
{
    private readonly ICurrentUser _currentUser;
    private readonly ISheetSectionDataService _sheetSectionData;

    public LiveSectionQuery(ICurrentUser currentUser, ISheetSectionDataService sheetSectionData)
    {
        _currentUser = currentUser;
        _sheetSectionData = sheetSectionData;
    }

    public async Task<IReadOnlyList<LiveSection>> VisibleAsync(int tableId, CancellationToken cancellationToken)
    {
        var me = _currentUser.UserId;
        var sections = await _sheetSectionData.ListPlacesOnTableAsync(tableId, cancellationToken);

        var visible = await _sheetSectionData.ListVisibleOnTableAsync(tableId, me, cancellationToken);
        var resolved = RevisionResolver.Resolve(visible, revision => revision.SheetSectionId, me);

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
                result.Add(new LiveSection
                {
                    Id = section.Id,
                    TemplateSectionId = section.TemplateSectionId,
                    ParentSheetSectionId = section.ParentSheetSectionId,
                    DisplayOrder = resolved[section.Id].Shown!.DisplayOrder,
                });
            }
        }

        return result;
    }

    public async Task<IReadOnlyCollection<int>> SubtreeIdsAsync(int tableId, int rootSectionId, CancellationToken cancellationToken)
    {
        var parents = await _sheetSectionData.GetParentsOnTableAsync(tableId, cancellationToken);

        var ids = new HashSet<int> { rootSectionId };
        var added = true;
        while (added)
        {
            added = false;
            foreach (var (id, parentId) in parents)
            {
                if (parentId is { } parent && ids.Contains(parent) && ids.Add(id))
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
