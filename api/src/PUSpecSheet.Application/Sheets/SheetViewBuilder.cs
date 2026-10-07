using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Turns a loaded sheet into the tree the screen shows: tables, their section copies (own rows first,
/// then sub-sections) and each row's cells with values. Anything whose latest shown revision removes it,
/// or sits inside something removed, is left out; so is anything only another person has drafted.
/// </summary>
internal sealed class SheetViewBuilder
{
    private readonly SheetSnapshot snapshot;
    private readonly int currentUserId;
    private readonly Dictionary<int, List<SheetSection>> topLevelByTable;
    private readonly Dictionary<int, List<SheetSection>> childrenByParent;
    private readonly Dictionary<int, List<SheetRow>> rowsBySection;
    private readonly HashSet<int> hiddenColumnBlockIds;

    public SheetViewBuilder(SheetSnapshot snapshot, int currentUserId)
    {
        this.snapshot = snapshot;
        this.currentUserId = currentUserId;

        var visibleSections = snapshot.Sections
            .Where(section => RevisionResolver.IsVisible(snapshot.SectionRevisions.GetValueOrDefault(section.Id)))
            .ToList();
        hiddenColumnBlockIds = snapshot.ColumnBlocks
            .Where(block => !RevisionResolver.IsVisible(snapshot.ColumnBlockRevisions.GetValueOrDefault(block.Id)))
            .Select(block => block.Id)
            .ToHashSet();
        topLevelByTable = visibleSections
            .Where(section => section.ParentSheetSectionId is null)
            .GroupBy(section => section.SheetTableId)
            .ToDictionary(group => group.Key, group => group.ToList());
        childrenByParent = visibleSections
            .Where(section => section.ParentSheetSectionId is not null)
            .GroupBy(section => section.ParentSheetSectionId!.Value)
            .ToDictionary(group => group.Key, group => group.ToList());
        rowsBySection = snapshot.Rows
            .Where(row => RevisionResolver.IsVisible(snapshot.RowRevisions.GetValueOrDefault(row.Id)))
            .GroupBy(row => row.SheetSectionId)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    public SheetDto Build()
    {
        var tables = snapshot.Tables
            .Where(table => RevisionResolver.IsVisible(snapshot.TableRevisions.GetValueOrDefault(table.Id)))
            .OrderBy(table => snapshot.TableRevisions[table.Id].Shown!.DisplayOrder)
            .ThenBy(table => table.Id)
            .Select(BuildTable)
            .ToList();

        var versions = snapshot.Versions
            .Select(version => new SheetVersionSummaryDto(
                version.VersionNumber,
                Utc(version.PublishedAtUtc),
                version.PublishedByUserId,
                version.PublishedBy.DisplayName,
                version.Note))
            .ToList();

        var templates = snapshot.AvailableTemplates
            .Select(template => new AvailableTemplateDto(
                template.Id,
                template.Name,
                snapshot.LatestTemplateVersions.GetValueOrDefault(template.Id, 1)))
            .ToList();

        var isLive = snapshot.AsOfUtc is null;
        var drafts = Drafts().ToList();
        return new SheetDto(
            snapshot.Sheet.Id,
            snapshot.Sheet.PublicId,
            snapshot.Sheet.PhaseId,
            snapshot.Sheet.SheetTypeId,
            isLive,
            snapshot.ViewedVersionNumber,
            snapshot.AsOfUtc is { } moment ? Utc(moment) : null,
            versions.Count == 0 ? null : versions[^1].VersionNumber,
            DraftSummaries.CountOf(drafts, currentUserId),
            versions,
            tables,
            templates)
        {
            OtherDrafts = DraftSummaries.OfOthers(drafts, currentUserId, UserName),
        };
    }

    private SheetTableDto BuildTable(SheetTable table)
    {
        var resolution = snapshot.TableRevisions[table.Id];
        var version = table.TableTemplateVersion;
        var topLevel = topLevelByTable.GetValueOrDefault(table.Id) ?? [];

        var sections = topLevel
            .OrderBy(section => section.TemplateSection.Role == SectionRole.Header ? 0 : 1)
            .ThenBy(section => snapshot.SectionRevisions[section.Id].Shown!.DisplayOrder)
            .ThenBy(section => section.Id)
            .Select(section => BuildSection(section, topLevel))
            .ToList();

        var blocks = BuildColumnBlocks(table);

        return new SheetTableDto(
            table.Id,
            table.PublicId,
            version.TableTemplateId,
            version.TableTemplate.Name,
            version.VersionNumber,
            version.Orientation,
            version.StickyColumnCount,
            resolution.Shown!.Title,
            resolution.Shown.DisplayOrder,
            LockOf(resolution),
            IsPending(resolution),
            sections,
            AddableSectionsUnder(version.Id, null, topLevel),
            blocks,
            AddableColumnBlocksFor(version.Id, blocks));
    }

    private SheetSectionDto BuildSection(SheetSection section, IReadOnlyList<SheetSection> siblings)
    {
        var resolution = snapshot.SectionRevisions[section.Id];
        var template = section.TemplateSection;
        var copies = siblings.Count(sibling => sibling.TemplateSectionId == section.TemplateSectionId);
        var children = childrenByParent.GetValueOrDefault(section.Id) ?? [];
        var isHeader = template.Role == SectionRole.Header;

        var rows = (rowsBySection.GetValueOrDefault(section.Id) ?? [])
            .OrderBy(row => snapshot.RowRevisions[row.Id].Shown!.DisplayOrder)
            .ThenBy(row => row.Id)
            .Select(row => BuildRow(row, isHeader))
            .ToList();

        var childSections = children
            .OrderBy(child => snapshot.SectionRevisions[child.Id].Shown!.DisplayOrder)
            .ThenBy(child => child.Id)
            .Select(child => BuildSection(child, children))
            .ToList();

        return new SheetSectionDto(
            section.Id,
            section.PublicId,
            section.TemplateSectionId,
            template.Name,
            template.Role,
            template.MinInstances,
            template.MaxInstances,
            template.InitialInstances,
            resolution.Shown!.DisplayOrder,
            LockOf(resolution),
            IsPending(resolution),
            !isHeader && copies > template.MinInstances,
            rows,
            childSections,
            AddableSectionsUnder(template.TableTemplateVersionId, template.Id, children),
            isHeader ? [] : AddableRows(template.Id),
            ChangeOf(snapshot.Changes.Sections, section.Id));
    }

    private SheetRowDto BuildRow(SheetRow row, bool inHeader)
    {
        var resolution = snapshot.RowRevisions[row.Id];
        var shown = resolution.Shown!;
        var values = snapshot.Values.GetValueOrDefault(shown.Id);

        var cells = row.Cells
            .Where(cell => cell.SheetColumnBlockId is not { } blockId || !hiddenColumnBlockIds.Contains(blockId))
            .OrderBy(cell => cell.TemplateCell.Column)
            .ThenBy(cell => cell.Id)
            .Select(cell => BuildCell(cell, values?.GetValueOrDefault(cell.Id), ChangeOf(snapshot.Changes.Cells, cell.Id)))
            .ToList();

        return new SheetRowDto(
            row.Id,
            row.PublicId,
            row.TemplateRowId,
            shown.DisplayOrder,
            LockOf(resolution),
            IsPending(resolution),
            !inHeader,
            cells,
            ChangeOf(snapshot.Changes.Rows, row.Id));
    }

    private static SheetCellDto BuildCell(SheetCell cell, CellValueBag? value, SheetChangeDto? change)
    {
        var template = cell.TemplateCell;
        return new SheetCellDto(
            cell.Id,
            cell.PublicId,
            new TemplateCellDto(
                template.Id,
                template.CellTypeId,
                template.Column,
                template.RowSpan,
                template.ColumnSpan,
                template.Caption,
                template.IsRequired,
                template.ConfigurationOverride,
                template.StyleOverride,
                template.TemplateColumnBlockId),
            value?.Text,
            value?.Number,
            value?.Date,
            value?.Boolean,
            value?.OptionId,
            cell.SheetColumnBlockId,
            change);
    }

    private List<SheetColumnBlockDto> BuildColumnBlocks(SheetTable table)
    {
        var visible = snapshot.ColumnBlocks
            .Where(block => block.SheetTableId == table.Id
                && RevisionResolver.IsVisible(snapshot.ColumnBlockRevisions.GetValueOrDefault(block.Id)))
            .ToList();

        return visible
            .OrderBy(block => snapshot.ColumnBlockRevisions[block.Id].Shown!.DisplayOrder)
            .ThenBy(block => block.Id)
            .Select(block =>
            {
                var resolution = snapshot.ColumnBlockRevisions[block.Id];
                var template = block.TemplateColumnBlock;
                var copies = visible.Count(candidate => candidate.TemplateColumnBlockId == block.TemplateColumnBlockId);
                return new SheetColumnBlockDto(
                    block.Id,
                    block.PublicId,
                    template.Id,
                    template.Name,
                    template.MinInstances,
                    template.MaxInstances,
                    template.InitialInstances,
                    template.StickyColumnCount,
                    resolution.Shown!.DisplayOrder,
                    LockOf(resolution),
                    IsPending(resolution),
                    copies > template.MinInstances,
                    ChangeOf(snapshot.Changes.ColumnBlocks, block.Id));
            })
            .ToList();
    }

    /// <summary>The kinds of column block the table can still take, with their current counts.</summary>
    private List<AddableColumnBlockDto> AddableColumnBlocksFor(int versionId, IReadOnlyList<SheetColumnBlockDto> existing)
    {
        return snapshot.TemplateColumnBlocks
            .Where(template => template.TableTemplateVersionId == versionId)
            .OrderBy(template => template.DisplayOrder)
            .ThenBy(template => template.Id)
            .Select(template =>
            {
                var count = existing.Count(block => block.TemplateColumnBlockId == template.Id);
                return new AddableColumnBlockDto(
                    template.Id,
                    template.Name,
                    count,
                    template.MinInstances,
                    template.MaxInstances,
                    template.MaxInstances is null || count < template.MaxInstances);
            })
            .ToList();
    }

    /// <summary>The kinds of section that can be added under a parent (or the table), with their current counts.</summary>
    private List<AddableSectionDto> AddableSectionsUnder(int versionId, int? parentTemplateSectionId, IReadOnlyList<SheetSection> existingCopies)
    {
        return snapshot.TemplateSections
            .Where(template => template.TableTemplateVersionId == versionId
                && template.ParentSectionId == parentTemplateSectionId
                && template.Role == SectionRole.Addable)
            .OrderBy(template => template.DisplayOrder)
            .ThenBy(template => template.Id)
            .Select(template =>
            {
                var count = existingCopies.Count(copy => copy.TemplateSectionId == template.Id);
                return new AddableSectionDto(
                    template.Id,
                    template.Name,
                    count,
                    template.MinInstances,
                    template.MaxInstances,
                    template.MaxInstances is null || count < template.MaxInstances);
            })
            .ToList();
    }

    private List<AddableRowDto> AddableRows(int templateSectionId)
    {
        return snapshot.TemplateRows
            .Where(row => row.TemplateSectionId == templateSectionId)
            .OrderBy(row => row.DisplayOrder)
            .ThenBy(row => row.Id)
            .Select((row, index) => new AddableRowDto(row.Id, RowLabel(row, index + 1)))
            .ToList();
    }

    private static string RowLabel(TemplateRow row, int position)
    {
        var caption = row.Cells
            .OrderBy(cell => cell.Column)
            .Select(cell => cell.Caption)
            .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
        return caption ?? $"Row {position}";
    }

    /// <summary>When and by whom an item last changed, or null if it never has.</summary>
    private SheetChangeDto? ChangeOf(Dictionary<int, SheetChange> changes, int itemId)
    {
        if (!changes.TryGetValue(itemId, out var change))
        {
            return null;
        }

        return new SheetChangeDto(change.VersionNumber, change.AtUtc, UserName(change.AuthorUserId));
    }

    private string UserName(int userId)
    {
        return snapshot.UserNames.GetValueOrDefault(userId, "Unknown user");
    }

    private SheetLockDto? LockOf<TRevision>(RevisionResolution<TRevision> resolution)
        where TRevision : class, ISheetRevision
    {
        if (resolution.Draft is not { } draft)
        {
            return null;
        }

        return new SheetLockDto(draft.AuthorUserId, UserName(draft.AuthorUserId), draft.AuthorUserId == currentUserId);
    }

    /// <summary>
    /// Whether the item has never been published, so only its author sees it. An item's first revision
    /// is number 1, so a draft with that number is the item's very first.
    /// </summary>
    private static bool IsPending<TRevision>(RevisionResolution<TRevision> resolution)
        where TRevision : class, ISheetRevision
    {
        return resolution.Shown is { Status: RevisionStatus.Draft, RevisionNumber: 1 };
    }

    /// <summary>Every draft on the sheet, whoever holds it. A past view has none.</summary>
    private IEnumerable<ISheetRevision> Drafts()
    {
        return snapshot.TableRevisions.Values.Select(resolution => (ISheetRevision?)resolution.Draft)
            .Concat(snapshot.SectionRevisions.Values.Select(resolution => resolution.Draft))
            .Concat(snapshot.RowRevisions.Values.Select(resolution => resolution.Draft))
            .Concat(snapshot.ColumnBlockRevisions.Values.Select(resolution => resolution.Draft))
            .OfType<ISheetRevision>();
    }

    private static DateTime Utc(DateTime value)
    {
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
