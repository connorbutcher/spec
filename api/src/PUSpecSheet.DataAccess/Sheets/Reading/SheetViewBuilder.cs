using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.History;
using PUSpecSheet.DataAccess.Sheets.Linking;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.Database.Mappers.Sheets;
using PUSpecSheet.Database.Mappers.Templates;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Reading;

/// <summary>
/// Turns a loaded sheet into the tree the screen shows: tables, their section copies (own rows first,
/// then sub-sections) and each row's cells with values. Anything whose latest shown revision removes it,
/// or sits inside something removed, is left out; so is anything only another person has drafted.
/// </summary>
internal sealed class SheetViewBuilder
{
    private readonly SheetSnapshot _snapshot;
    private readonly int _currentUserId;
    private readonly Dictionary<int, List<DbSheetSection>> _topLevelByTable;
    private readonly Dictionary<int, List<DbSheetSection>> _childrenByParent;
    private readonly Dictionary<int, List<DbSheetRow>> _rowsBySection;
    private readonly HashSet<int> _hiddenColumnBlockIds;
    private readonly Dictionary<int, int> _tableBySection;
    private readonly Dictionary<int, List<SheetLinkableColumnDto>> _linkableColumnsByVersion = [];
    private readonly LinkedOptionCollector _linkedOptions;
    private readonly Dictionary<int, int> _columnBlockOrder;
    private List<SheetLinkableRowDto> _linkableRows = [];

    /// <summary>The template cells the sheet's cells were built from, each sent once, by id.</summary>
    private readonly Dictionary<int, TemplateCellDto> _templateCells = [];

    public SheetViewBuilder(SheetSnapshot snapshot, int currentUserId)
    {
        _snapshot = snapshot;
        _currentUserId = currentUserId;
        _tableBySection = snapshot.Sections.ToDictionary(section => section.Id, section => section.SheetTableId);
        _linkedOptions = new LinkedOptionCollector(snapshot.Values.Values.SelectMany(cells => cells.Values));
        _columnBlockOrder = snapshot.ColumnBlocks.ToDictionary(
            block => block.Id,
            block => snapshot.ColumnBlockRevisions.GetValueOrDefault(block.Id)?.Shown?.DisplayOrder ?? 0);

        var visibleSections = snapshot.Sections
            .Where(section => RevisionResolver.IsVisible(snapshot.SectionRevisions.GetValueOrDefault(section.Id)))
            .ToList();
        _hiddenColumnBlockIds = snapshot.ColumnBlocks
            .Where(block => !RevisionResolver.IsVisible(snapshot.ColumnBlockRevisions.GetValueOrDefault(block.Id)))
            .Select(block => block.Id)
            .ToHashSet();
        _topLevelByTable = visibleSections
            .Where(section => section.ParentSheetSectionId is null)
            .GroupBy(section => section.SheetTableId)
            .ToDictionary(group => group.Key, group => group.ToList());
        _childrenByParent = visibleSections
            .Where(section => section.ParentSheetSectionId is not null)
            .GroupBy(section => section.ParentSheetSectionId!.Value)
            .ToDictionary(group => group.Key, group => group.ToList());
        _rowsBySection = snapshot.Rows
            .Where(row => RevisionResolver.IsVisible(snapshot.RowRevisions.GetValueOrDefault(row.Id)))
            .GroupBy(row => row.SheetSectionId)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    public SheetDto Build()
    {
        var tables = BuildTables();
        var versions = BuildVersions();
        var templates = BuildAvailableTemplates();
        var isLive = _snapshot.AsOfUtc is null && _snapshot.ReviewedSubmissionId is null;
        var drafts = Drafts().ToList();
        return new SheetDto
        {
            Id = _snapshot.Sheet.Id,
            PublicId = _snapshot.Sheet.PublicId,
            PhaseId = _snapshot.Sheet.PhaseId,
            SheetTypeId = _snapshot.Sheet.SheetTypeId,
            IsLive = isLive,
            ViewedVersionNumber = _snapshot.ViewedVersionNumber,
            ViewedAsOfUtc = _snapshot.AsOfUtc is { } moment ? Utc(moment) : null,
            LatestVersionNumber = versions.Count == 0 ? null : versions[^1].VersionNumber,
            MyDraftCount = DraftSummaries.CountOf(drafts, _currentUserId),
            Versions = versions,
            Tables = tables,
            AvailableTemplates = templates,
            OtherDrafts = DraftSummaries.OfOthers(drafts, _currentUserId, UserName),
            Identifiers = [.. _snapshot.Sheet.IdentifierValues.OrderBy(value => value.SheetTypeIdentifierId).Select(value => new SheetIdentifierValueDto
            {
                IdentifierId = value.SheetTypeIdentifierId,
                Value = value.Value,
            })],
            ReviewedSubmissionId = _snapshot.ReviewedSubmissionId,
            // Gathered while the tables above were built, so they hold exactly what the viewer can see.
                LinkedSources = _linkedOptions.Build(),
            // Gathered with the cells, so it holds the template of every cell shown and nothing more.
                TemplateCells = [.. _templateCells.Values.OrderBy(templateCell => templateCell.Id)],
        };
    }

    private List<SheetTableDto> BuildTables()
    {
        return _snapshot.Tables
            .Where(table => RevisionResolver.IsVisible(_snapshot.TableRevisions.GetValueOrDefault(table.Id)))
            .OrderBy(table => _snapshot.TableRevisions[table.Id].Shown!.DisplayOrder)
            .ThenBy(table => table.Id)
            .Select(BuildTable)
            .ToList();
    }

    private List<SheetVersionSummaryDto> BuildVersions()
    {
        return _snapshot.Versions
            .Select(version => version.ToSummaryDto())
            .ToList();
    }

    private List<AvailableTemplateDto> BuildAvailableTemplates()
    {
        return _snapshot.AvailableTemplates
            .Select(template => new AvailableTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                LatestVersionNumber = _snapshot.LatestTemplateVersions.GetValueOrDefault(template.Id, 1),
            })
            .ToList();
    }

    private SheetTableDto BuildTable(DbSheetTable table)
    {
        var resolution = _snapshot.TableRevisions[table.Id];
        var version = table.TableTemplateVersion;
        var topLevel = _topLevelByTable.GetValueOrDefault(table.Id) ?? [];

        // Filled as the table's rows are built just below, so it is in the order they appear.
        _linkableRows = [];
        var sections = topLevel
            .OrderBy(section => section.TemplateSection.Role == SectionRole.Header ? 0 : 1)
            .ThenBy(section => _snapshot.SectionRevisions[section.Id].Shown!.DisplayOrder)
            .ThenBy(section => section.Id)
            .Select(section => BuildSection(section, topLevel))
            .ToList();

        var blocks = BuildColumnBlocks(table);

        return new SheetTableDto
        {
            Id = table.Id,
            PublicId = table.PublicId,
            TableTemplateId = version.TableTemplateId,
            TemplateName = version.TableTemplate.Name,
            TemplateVersionNumber = version.VersionNumber,
            Orientation = version.Orientation,
            StickyColumnCount = version.StickyColumnCount,
            Title = resolution.Shown!.Title,
            DisplayOrder = resolution.Shown.DisplayOrder,
            Lock = LockOf(resolution),
            IsPending = IsPending(resolution),
            Sections = sections,
            AddableSections = AddableSectionsUnder(version.Id, null, topLevel),
            ColumnBlocks = blocks,
            AddableColumnBlocks = AddableColumnBlocksFor(version.Id, blocks),
            LinkableColumns = LinkableColumnsOf(version.Id),
            LinkableRows = _linkableRows,
        };
    }

    /// <summary>The columns of a template version that can be linked to, worked out once per version.</summary>
    private List<SheetLinkableColumnDto> LinkableColumnsOf(int versionId)
    {
        if (!_linkableColumnsByVersion.TryGetValue(versionId, out var columns))
        {
            var sections = _snapshot.TemplateSections.Where(section => section.TableTemplateVersionId == versionId).ToList();
            columns = LinkableColumns.For(sections, _snapshot.TemplateRows, _snapshot.CellTypes);
            _linkableColumnsByVersion[versionId] = columns;
        }

        return columns;
    }

    private SheetSectionDto BuildSection(DbSheetSection section, IReadOnlyList<DbSheetSection> siblings)
    {
        var resolution = _snapshot.SectionRevisions[section.Id];
        var template = section.TemplateSection;
        var copies = siblings.Count(sibling => sibling.TemplateSectionId == section.TemplateSectionId);
        var children = _childrenByParent.GetValueOrDefault(section.Id) ?? [];
        var isHeader = template.Role == SectionRole.Header;

        var rows = (_rowsBySection.GetValueOrDefault(section.Id) ?? [])
            .OrderBy(row => _snapshot.RowRevisions[row.Id].Shown!.DisplayOrder)
            .ThenBy(row => row.Id)
            .Select(row => BuildRow(row, isHeader))
            .ToList();

        var childSections = children
            .OrderBy(child => _snapshot.SectionRevisions[child.Id].Shown!.DisplayOrder)
            .ThenBy(child => child.Id)
            .Select(child => BuildSection(child, children))
            .ToList();

        return new SheetSectionDto
        {
            Id = section.Id,
            PublicId = section.PublicId,
            TemplateSectionId = section.TemplateSectionId,
            Name = template.Name,
            Role = template.Role,
            MinInstances = template.MinInstances,
            MaxInstances = template.MaxInstances,
            InitialInstances = template.InitialInstances,
            DisplayOrder = resolution.Shown!.DisplayOrder,
            Lock = LockOf(resolution),
            IsPending = IsPending(resolution),
            CanRemove = !isHeader && copies > template.MinInstances,
            Rows = rows,
            Sections = childSections,
            AddableSections = AddableSectionsUnder(template.TableTemplateVersionId, template.Id, children),
            LastChange = ChangeOf(_snapshot.Changes.Sections, section.Id),
        };
    }

    private SheetRowDto BuildRow(DbSheetRow row, bool inHeader)
    {
        var resolution = _snapshot.RowRevisions[row.Id];
        var shown = resolution.Shown!;
        var values = _snapshot.Values.GetValueOrDefault(shown.Id);
        var tableId = _tableBySection[row.SheetSectionId];

        var onSheet = row.Cells
            .Where(cell => cell.SheetColumnBlockId is not { } blockId || !_hiddenColumnBlockIds.Contains(blockId))
            .ToList();
        OfferRowToLinkedDropdowns(tableId, row, onSheet, values);

        var cells = onSheet
            .OrderBy(cell => cell.TemplateCell.Column)
            .ThenBy(cell => cell.Id)
            .Select(cell =>
            {
                var value = values?.GetValueOrDefault(cell.Id);
                OfferToLinkedDropdowns(tableId, cell, value);
                return BuildCell(cell, value, ChangeOf(_snapshot.Changes.Cells, cell.Id));
            })
            .ToList();

        return new SheetRowDto
        {
            Id = row.Id,
            PublicId = row.PublicId,
            TemplateRowId = row.TemplateRowId,
            DisplayOrder = shown.DisplayOrder,
            Lock = LockOf(resolution),
            IsPending = IsPending(resolution),
            CanRemove = !inHeader,
            Cells = cells,
            LastChange = ChangeOf(_snapshot.Changes.Rows, row.Id),
        };
    }

    private SheetCellDto BuildCell(DbSheetCell cell, CellValueBag? value, SheetChangeDto? change)
    {
        var template = cell.TemplateCell;
        if (!_templateCells.ContainsKey(template.Id))
        {
            _templateCells[template.Id] = template.ToDto();
        }

        return new SheetCellDto
        {
            Id = cell.Id,
            PublicId = cell.PublicId,
            TemplateCellId = template.Id,
            Value = CellValueMapper.ToValue(value),
            SheetColumnBlockId = cell.SheetColumnBlockId,
            LastChange = change,
            Settings = value?.Settings,
        };
    }

    /// <summary>Adds a visible cell's value to the choices of its column, when a linked dropdown is pointed at it.</summary>
    private void OfferToLinkedDropdowns(int tableId, DbSheetCell cell, CellValueBag? value)
    {
        if (_linkedOptions.Wants(tableId, cell.TemplateCellId)
            && _snapshot.CellTypes.TryGetValue(cell.TemplateCell.CellTypeId, out var cellType)
            && LinkedOptionText.Of(value, cellType, cell.TemplateCell.ConfigurationOverride) is { } text)
        {
            _linkedOptions.Add(tableId, cell.TemplateCellId, text);
        }
    }

    /// <summary>
    /// Lists a visible row among the rows that can be linked to, under its name, and when a linked dropdown
    /// is pointed at it adds its other values to that dropdown's choices. The values are read left to right:
    /// the row's own cells, then each set of repeated columns.
    /// </summary>
    private void OfferRowToLinkedDropdowns(int tableId, DbSheetRow row, List<DbSheetCell> cells, Dictionary<int, CellValueBag>? values)
    {
        var texts = cells
            .OrderBy(cell => cell.SheetColumnBlockId is { } blockId ? 1 + _columnBlockOrder.GetValueOrDefault(blockId) : 0)
            .ThenBy(cell => cell.SheetColumnBlockId ?? 0)
            .ThenBy(cell => cell.TemplateCell.Column)
            .ThenBy(cell => cell.Id)
            .Select(cell => _snapshot.CellTypes.TryGetValue(cell.TemplateCell.CellTypeId, out var cellType) && cellType.Kind.CanBeLinkedTo()
                ? LinkedOptionText.Of(values?.GetValueOrDefault(cell.Id), cellType, cell.TemplateCell.ConfigurationOverride)
                : null)
            .OfType<string>()
            .ToList();
        if (LinkedRowValues.NameOf(texts) is not { } name)
        {
            return;
        }

        _linkableRows.Add(new SheetLinkableRowDto { SheetRowId = row.Id, Label = name });
        if (_linkedOptions.WantsRow(tableId, row.Id))
        {
            foreach (var text in LinkedRowValues.ChoicesOf(texts))
            {
                _linkedOptions.AddToRow(tableId, row.Id, text);
            }
        }
    }

    private List<SheetColumnBlockDto> BuildColumnBlocks(DbSheetTable table)
    {
        var visible = _snapshot.ColumnBlocks
            .Where(block => block.SheetTableId == table.Id
                && RevisionResolver.IsVisible(_snapshot.ColumnBlockRevisions.GetValueOrDefault(block.Id)))
            .ToList();

        return visible
            .OrderBy(block => _snapshot.ColumnBlockRevisions[block.Id].Shown!.DisplayOrder)
            .ThenBy(block => block.Id)
            .Select(block =>
            {
                var resolution = _snapshot.ColumnBlockRevisions[block.Id];
                var template = block.TemplateColumnBlock;
                var copies = visible.Count(candidate => candidate.TemplateColumnBlockId == block.TemplateColumnBlockId);
                return new SheetColumnBlockDto
                {
                    Id = block.Id,
                    PublicId = block.PublicId,
                    TemplateColumnBlockId = template.Id,
                    Name = template.Name,
                    MinInstances = template.MinInstances,
                    MaxInstances = template.MaxInstances,
                    InitialInstances = template.InitialInstances,
                    StickyColumnCount = template.StickyColumnCount,
                    DisplayOrder = resolution.Shown!.DisplayOrder,
                    Lock = LockOf(resolution),
                    IsPending = IsPending(resolution),
                    CanRemove = copies > template.MinInstances,
                    LastChange = ChangeOf(_snapshot.Changes.ColumnBlocks, block.Id),
                };
            })
            .ToList();
    }

    /// <summary>The kinds of column block the table can still take, with their current counts.</summary>
    private List<AddableColumnBlockDto> AddableColumnBlocksFor(int versionId, IReadOnlyList<SheetColumnBlockDto> existing)
    {
        return _snapshot.TemplateColumnBlocks
            .Where(template => template.TableTemplateVersionId == versionId)
            .OrderBy(template => template.DisplayOrder)
            .ThenBy(template => template.Id)
            .Select(template =>
            {
                var count = existing.Count(block => block.TemplateColumnBlockId == template.Id);
                return new AddableColumnBlockDto
                {
                    TemplateColumnBlockId = template.Id,
                    Name = template.Name,
                    Count = count,
                    MinInstances = template.MinInstances,
                    MaxInstances = template.MaxInstances,
                    CanAdd = template.MaxInstances is null || count < template.MaxInstances,
                };
            })
            .ToList();
    }

    /// <summary>The kinds of section that can be added under a parent (or the table), with their current counts.</summary>
    private List<AddableSectionDto> AddableSectionsUnder(int versionId, int? parentTemplateSectionId, IReadOnlyList<DbSheetSection> existingCopies)
    {
        return _snapshot.TemplateSections
            .Where(template => template.TableTemplateVersionId == versionId
                && template.ParentSectionId == parentTemplateSectionId
                && template.Role == SectionRole.Addable)
            .OrderBy(template => template.DisplayOrder)
            .ThenBy(template => template.Id)
            .Select(template =>
            {
                var count = existingCopies.Count(copy => copy.TemplateSectionId == template.Id);
                return new AddableSectionDto
                {
                    TemplateSectionId = template.Id,
                    Name = template.Name,
                    Count = count,
                    MinInstances = template.MinInstances,
                    MaxInstances = template.MaxInstances,
                    CanAdd = template.MaxInstances is null || count < template.MaxInstances,
                };
            })
            .ToList();
    }

    /// <summary>When and by whom an item last changed, or null if it never has.</summary>
    private SheetChangeDto? ChangeOf(Dictionary<int, SheetChange> changes, int itemId)
    {
        if (!changes.TryGetValue(itemId, out var change))
        {
            return null;
        }

        return new SheetChangeDto { VersionNumber = change.VersionNumber, AtUtc = change.AtUtc, UserName = UserName(change.AuthorUserId) };
    }

    private string UserName(int userId)
    {
        return _snapshot.UserNames.GetValueOrDefault(userId, "Unknown user");
    }

    private SheetLockDto? LockOf<TRevision>(RevisionResolution<TRevision> resolution)
        where TRevision : class, IDbSheetRevision
    {
        if (resolution.Draft is not { } draft)
        {
            return null;
        }

        return new SheetLockDto
        {
            UserId = draft.AuthorUserId,
            UserName = UserName(draft.AuthorUserId),
            IsMine = draft.AuthorUserId == _currentUserId,
            IsAwaitingApproval = draft.SheetSubmissionId is not null,
        };
    }

    /// <summary>
    /// Whether the item has never been published, so only its author sees it. An item's first revision
    /// is number 1, so a draft with that number is the item's very first.
    /// </summary>
    private static bool IsPending<TRevision>(RevisionResolution<TRevision> resolution)
        where TRevision : class, IDbSheetRevision
    {
        return resolution.Shown is { Status: RevisionStatus.Draft, RevisionNumber: 1 };
    }

    /// <summary>
    /// Every draft on the sheet that can still be published, whoever holds it. A past view has none, and drafts
    /// already waiting for approval belong to their submission.
    /// </summary>
    private IEnumerable<IDbSheetRevision> Drafts()
    {
        return _snapshot.TableRevisions.Values.Select(resolution => (IDbSheetRevision?)resolution.Draft)
            .Concat(_snapshot.SectionRevisions.Values.Select(resolution => resolution.Draft))
            .Concat(_snapshot.RowRevisions.Values.Select(resolution => resolution.Draft))
            .Concat(_snapshot.ColumnBlockRevisions.Values.Select(resolution => resolution.Draft))
            .OfType<IDbSheetRevision>()
            .Where(draft => draft.SheetSubmissionId is null);
    }

    private static DateTime Utc(DateTime value)
    {
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
