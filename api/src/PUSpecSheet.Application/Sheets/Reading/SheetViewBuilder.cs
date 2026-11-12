using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.History;
using PUSpecSheet.Application.Sheets.Linking;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data.Values;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Reading;

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
    private readonly Dictionary<int, int> tableBySection;
    private readonly Dictionary<int, List<SheetLinkableColumnDto>> linkableColumnsByVersion = [];
    private readonly LinkedOptionCollector linkedOptions;
    private readonly Dictionary<int, int> columnBlockOrder;
    private List<SheetLinkableRowDto> linkableRows = [];

    /// <summary>The template cells the sheet's cells were built from, each sent once, by id.</summary>
    private readonly Dictionary<int, TemplateCellDto> templateCells = [];

    public SheetViewBuilder(SheetSnapshot snapshot, int currentUserId)
    {
        this.snapshot = snapshot;
        this.currentUserId = currentUserId;
        tableBySection = snapshot.Sections.ToDictionary(section => section.Id, section => section.SheetTableId);
        linkedOptions = new LinkedOptionCollector(snapshot.Values.Values.SelectMany(cells => cells.Values));
        columnBlockOrder = snapshot.ColumnBlocks.ToDictionary(
            block => block.Id,
            block => snapshot.ColumnBlockRevisions.GetValueOrDefault(block.Id)?.Shown?.DisplayOrder ?? 0);

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
        var tables = BuildTables();
        var versions = BuildVersions();
        var templates = BuildAvailableTemplates();
        var isLive = snapshot.AsOfUtc is null && snapshot.ReviewedSubmissionId is null;
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
            ReviewedSubmissionId = snapshot.ReviewedSubmissionId,

            // Gathered while the tables above were built, so they hold exactly what the viewer can see.
            LinkedSources = linkedOptions.Build(),

            // Gathered with the cells, so it holds the template of every cell shown and nothing more.
            TemplateCells = [.. templateCells.Values.OrderBy(templateCell => templateCell.Id)],
        };
    }

    private List<SheetTableDto> BuildTables()
    {
        return snapshot.Tables
            .Where(table => RevisionResolver.IsVisible(snapshot.TableRevisions.GetValueOrDefault(table.Id)))
            .OrderBy(table => snapshot.TableRevisions[table.Id].Shown!.DisplayOrder)
            .ThenBy(table => table.Id)
            .Select(BuildTable)
            .ToList();
    }

    private List<SheetVersionSummaryDto> BuildVersions()
    {
        return snapshot.Versions
            .Select(version => new SheetVersionSummaryDto(
                version.VersionNumber,
                Utc(version.PublishedAtUtc),
                version.PublishedByUserId,
                version.PublishedBy.DisplayName,
                version.Note))
            .ToList();
    }

    private List<AvailableTemplateDto> BuildAvailableTemplates()
    {
        return snapshot.AvailableTemplates
            .Select(template => new AvailableTemplateDto(
                template.Id,
                template.Name,
                snapshot.LatestTemplateVersions.GetValueOrDefault(template.Id, 1)))
            .ToList();
    }

    private SheetTableDto BuildTable(SheetTable table)
    {
        var resolution = snapshot.TableRevisions[table.Id];
        var version = table.TableTemplateVersion;
        var topLevel = topLevelByTable.GetValueOrDefault(table.Id) ?? [];

        // Filled as the table's rows are built just below, so it is in the order they appear.
        linkableRows = [];
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
            AddableColumnBlocksFor(version.Id, blocks))
        {
            LinkableColumns = LinkableColumnsOf(version.Id),
            LinkableRows = linkableRows,
        };
    }

    /// <summary>The columns of a template version that can be linked to, worked out once per version.</summary>
    private List<SheetLinkableColumnDto> LinkableColumnsOf(int versionId)
    {
        if (!linkableColumnsByVersion.TryGetValue(versionId, out var columns))
        {
            var sections = snapshot.TemplateSections.Where(section => section.TableTemplateVersionId == versionId).ToList();
            columns = LinkableColumns.For(sections, snapshot.TemplateRows, snapshot.CellTypes);
            linkableColumnsByVersion[versionId] = columns;
        }

        return columns;
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
        var tableId = tableBySection[row.SheetSectionId];

        var onSheet = row.Cells
            .Where(cell => cell.SheetColumnBlockId is not { } blockId || !hiddenColumnBlockIds.Contains(blockId))
            .ToList();
        OfferRowToLinkedDropdowns(tableId, row, onSheet, values);

        var cells = onSheet
            .OrderBy(cell => cell.TemplateCell.Column)
            .ThenBy(cell => cell.Id)
            .Select(cell =>
            {
                var value = values?.GetValueOrDefault(cell.Id);
                OfferToLinkedDropdowns(tableId, cell, value);
                return BuildCell(cell, value, ChangeOf(snapshot.Changes.Cells, cell.Id));
            })
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

    private SheetCellDto BuildCell(SheetCell cell, CellValueBag? value, SheetChangeDto? change)
    {
        var template = cell.TemplateCell;
        if (!templateCells.ContainsKey(template.Id))
        {
            templateCells[template.Id] = new TemplateCellDto(
                template.Id,
                template.CellTypeId,
                template.Column,
                template.RowSpan,
                template.ColumnSpan,
                template.Caption,
                template.IsRequired,
                template.ConfigurationOverride,
                template.StyleOverride,
                template.TemplateColumnBlockId);
        }

        return new SheetCellDto(
            cell.Id,
            cell.PublicId,
            template.Id,
            CellValueMapper.ToValue(value),
            cell.SheetColumnBlockId,
            change)
        {
            Settings = value?.Settings,
        };
    }

    /// <summary>Adds a visible cell's value to the choices of its column, when a linked dropdown is pointed at it.</summary>
    private void OfferToLinkedDropdowns(int tableId, SheetCell cell, CellValueBag? value)
    {
        if (linkedOptions.Wants(tableId, cell.TemplateCellId)
            && snapshot.CellTypes.TryGetValue(cell.TemplateCell.CellTypeId, out var cellType)
            && LinkedOptionText.Of(value, cellType, cell.TemplateCell.ConfigurationOverride) is { } text)
        {
            linkedOptions.Add(tableId, cell.TemplateCellId, text);
        }
    }

    /// <summary>
    /// Lists a visible row among the rows that can be linked to, under its name, and when a linked dropdown
    /// is pointed at it adds its other values to that dropdown's choices. The values are read left to right:
    /// the row's own cells, then each set of repeated columns.
    /// </summary>
    private void OfferRowToLinkedDropdowns(int tableId, SheetRow row, List<SheetCell> cells, Dictionary<int, CellValueBag>? values)
    {
        var texts = cells
            .OrderBy(cell => cell.SheetColumnBlockId is { } blockId ? 1 + columnBlockOrder.GetValueOrDefault(blockId) : 0)
            .ThenBy(cell => cell.SheetColumnBlockId ?? 0)
            .ThenBy(cell => cell.TemplateCell.Column)
            .ThenBy(cell => cell.Id)
            .Select(cell => snapshot.CellTypes.TryGetValue(cell.TemplateCell.CellTypeId, out var cellType) && cellType.Kind.CanBeLinkedTo()
                ? LinkedOptionText.Of(values?.GetValueOrDefault(cell.Id), cellType, cell.TemplateCell.ConfigurationOverride)
                : null)
            .OfType<string>()
            .ToList();
        if (LinkedRowValues.NameOf(texts) is not { } name)
        {
            return;
        }

        linkableRows.Add(new SheetLinkableRowDto(row.Id, name));
        if (linkedOptions.WantsRow(tableId, row.Id))
        {
            foreach (var text in LinkedRowValues.ChoicesOf(texts))
            {
                linkedOptions.AddToRow(tableId, row.Id, text);
            }
        }
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

        return new SheetLockDto(draft.AuthorUserId, UserName(draft.AuthorUserId), draft.AuthorUserId == currentUserId)
        {
            IsAwaitingApproval = draft.SheetSubmissionId is not null,
        };
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

    /// <summary>
    /// Every draft on the sheet that can still be published, whoever holds it. A past view has none, and drafts
    /// already waiting for approval belong to their submission.
    /// </summary>
    private IEnumerable<ISheetRevision> Drafts()
    {
        return snapshot.TableRevisions.Values.Select(resolution => (ISheetRevision?)resolution.Draft)
            .Concat(snapshot.SectionRevisions.Values.Select(resolution => resolution.Draft))
            .Concat(snapshot.RowRevisions.Values.Select(resolution => resolution.Draft))
            .Concat(snapshot.ColumnBlockRevisions.Values.Select(resolution => resolution.Draft))
            .OfType<ISheetRevision>()
            .Where(draft => draft.SheetSubmissionId is null);
    }

    private static DateTime Utc(DateTime value)
    {
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
