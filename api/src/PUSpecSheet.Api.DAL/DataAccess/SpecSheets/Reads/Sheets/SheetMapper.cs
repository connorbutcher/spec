using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Configuration;
using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Mappers.Configuration;
using PUSpecSheet.Api.Database.Mappers.Template;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Sheets;

/// <summary>
/// Maps what the revisions show of a sheet to the <see cref="Sheet"/> models, which keep the revision schema's shape:
/// every item has its own id and the revision of it that is shown, and a cell has the value of its row's revision, of
/// the kind its cell type holds. Where an item has a draft that is the revision shown, otherwise its published one.
/// </summary>
internal class SheetMapper
{
    private readonly Dictionary<int, User> _users;
    private readonly Dictionary<int, int> _versionNumbers;
    private readonly Dictionary<int, TableCellType> _cellTypes = [];
    private readonly Dictionary<int, CellConfiguration> _cellConfigurations = [];

    public SheetMapper(IEnumerable<DbUser> users, IEnumerable<DbSheetVersion> versions)
    {
        ArgumentNullException.ThrowIfNull(users, nameof(users));
        ArgumentNullException.ThrowIfNull(versions, nameof(versions));

        _users = users.ToDictionary(user => user.Id, user => user.MapToDomain());
        _versionNumbers = versions.ToDictionary(version => version.Id, version => version.VersionNumber);
    }

    /// <summary>
    /// The ids of the users a mapped sheet names, so that they can be read before mapping.
    /// </summary>
    public static IEnumerable<int> UserIds(DbSheet sheet, IEnumerable<DbSheetVersion> versions, IEnumerable<DbSheetLinkedTable> linkedTables)
    {
        var revisions = new List<IDbSheetRevision>();
        revisions.AddRange(sheet.Tables.Select(table => table.Revisions.Shown()));
        revisions.AddRange(sheet.Tables.SelectMany(table => table.Groups).Select(group => group.Revisions.Shown()));
        revisions.AddRange(sheet.Tables.SelectMany(table => table.Groups).SelectMany(group => group.Rows).Select(row => row.Revisions.Shown()));
        revisions.AddRange(sheet.Properties.Select(property => property.Revisions.Shown()));
        revisions.AddRange(linkedTables.Select(linked => linked.Revisions.Shown()));

        return revisions.Select(revision => revision.CreatedByUserId)
            .Concat(sheet.Tables.SelectMany(table => table.Revisions).SelectMany(revision => revision.NoteValues).Select(note => note.SheetTableNote!.CreatedByUserId))
            .Concat(linkedTables.Select(linked => linked.LinkedByUserId))
            .Concat(versions.Select(version => version.PublishedByUserId));
    }

    /// <summary>
    /// The ids of the users a mapped summary names: those who published its versions and wrote its properties.
    /// </summary>
    public static IEnumerable<int> SummaryUserIds(IEnumerable<DbSheet> sheets)
    {
        return sheets.SelectMany(sheet => sheet.Versions.Select(version => version.PublishedByUserId)
            .Concat(sheet.Properties.Select(property => property.Revisions.Shown().CreatedByUserId)));
    }

    /// <summary>
    /// The sheet without its tables, with its latest version and the properties its current published revisions give.
    /// </summary>
    public SheetSummary MapSummary(DbSheet sheet, int? draftSpecSheetId)
    {
        var latest = sheet.Versions.OrderByDescending(version => version.VersionNumber).FirstOrDefault();
        return new SheetSummary
        {
            Id = sheet.Id,
            ReferenceId = sheet.ReferenceId,
            SpecSheetPhaseId = sheet.SpecSheetPhaseId,
            SpecSheetTypeId = sheet.SpecSheetTypeId,
            CreatedDateTime = sheet.CreatedDateTime,
            SpecSheetPhase = sheet.SpecSheetPhase?.MapToDomain(),
            SpecSheetType = sheet.SpecSheetType?.MapToDomain(),
            LatestVersion = latest is null ? null : MapVersion(latest),
            DraftSpecSheetId = draftSpecSheetId,
            Properties = [.. sheet.Properties
                .Where(property => property.Revisions.IsShown())
                .Select(MapProperty)
                .OrderBy(property => property.Id)]
        };
    }

    public Sheet Map(DbSheet sheet, DbSheetVersion? version, ICollection<DbSheetLinkedTable> linkedTables, DateTime asOf, bool isDraft)
    {
        return new Sheet
        {
            Id = sheet.Id,
            ReferenceId = sheet.ReferenceId,
            SpecSheetPhaseId = sheet.SpecSheetPhaseId,
            SpecSheetTypeId = sheet.SpecSheetTypeId,
            CreatedDateTime = sheet.CreatedDateTime,
            Version = version is null ? null : MapVersion(version),
            AsOf = asOf,
            IsDraft = isDraft,
            SpecSheetPhase = sheet.SpecSheetPhase?.MapToDomain(),
            SpecSheetType = sheet.SpecSheetType?.MapToDomain(),
            Tables = [.. sheet.Tables
                .Where(table => table.Revisions.IsShown())
                .Select(MapTable)
                .OrderBy(table => table.DisplayOrder)
                .ThenBy(table => table.Id)],
            Properties = [.. sheet.Properties
                .Where(property => property.Revisions.IsShown())
                .Select(MapProperty)
                .OrderBy(property => property.Id)],
            LinkedTables = [.. linkedTables
                .Where(linked => linked.Revisions.IsShown())
                .Select(MapLinkedTable)
                .OrderBy(linked => linked.DisplayOrder)
                .ThenBy(linked => linked.Id)],
            ExternalReferences = [.. sheet.ExternalReferences
                .Where(reference => reference.Revisions.Count > 0)
                .Select(MapExternalReference)
                .OrderBy(reference => reference.Id)]
        };
    }

    public SheetVersion MapVersion(DbSheetVersion version)
    {
        return new SheetVersion
        {
            Id = version.Id,
            SheetId = version.SheetId,
            VersionNumber = version.VersionNumber,
            PublishedDateTime = version.PublishedDateTime,
            PublishedByUserId = version.PublishedByUserId,
            PublishedByUser = User(version.PublishedByUserId),
            ChangeLog = version.ChangeLog,
            HasBeenMigrated = version.HasBeenMigrated,
            SourceSpecSheetId = version.SourceSpecSheetId,
            LegacySpecSheetId = version.LegacySpecSheetId,
            SourceCreatedByUserId = version.SourceCreatedByUserId,
            SourceCreatedDateTime = version.SourceCreatedDateTime
        };
    }

    /// <summary>
    /// A submission with the users its includes brought, for a mapper that wasn't given them.
    /// </summary>
    public SheetSubmission MapSubmission(DbSheetSubmission submission)
    {
        return new SheetSubmission
        {
            Id = submission.Id,
            SheetId = submission.SheetId,
            Note = submission.Note,
            SubmittedByUserId = submission.SubmittedByUserId,
            SubmittedByUser = submission.SubmittedByUser?.MapToDomain(),
            SubmittedDateTime = submission.SubmittedDateTime,
            Status = submission.Status,
            DecidedDateTime = submission.DecidedDateTime,
            SheetVersionId = submission.SheetVersionId,
            SheetVersionNumber = submission.SheetVersionId is { } versionId && _versionNumbers.TryGetValue(versionId, out var number) ? number : null,
            Approvers = [.. submission.Approvers
                .OrderBy(approver => approver.UserId)
                .Select(approver => new SheetSubmissionApprover
                {
                    SheetSubmissionId = approver.SheetSubmissionId,
                    UserId = approver.UserId,
                    User = approver.User?.MapToDomain(),
                    Decision = approver.Decision,
                    DecidedDateTime = approver.DecidedDateTime,
                    Reason = approver.Reason == DbSheetSubmissionApprover.NoReasonGiven ? null : approver.Reason
                })]
        };
    }

    private User? User(int userId)
    {
        return _users.GetValueOrDefault(userId);
    }

    private SheetItemRevision MapRevision(IDbSheetRevision revision)
    {
        return new SheetItemRevision
        {
            Id = revision.Id,
            RevisionNumber = revision.RevisionNumber,
            Status = revision.Status,
            CreatedByUserId = revision.CreatedByUserId,
            CreatedDateTime = revision.CreatedDateTime,
            CreatedByUser = User(revision.CreatedByUserId),
            LastModifiedDateTime = revision.LastModifiedDateTime,
            PublishedDateTime = revision.PublishedDateTime,
            SupersededDateTime = revision.SupersededDateTime,
            SheetVersionId = revision.SheetVersionId,
            SheetVersionNumber = revision.SheetVersionId is { } versionId && _versionNumbers.TryGetValue(versionId, out var number) ? number : null
        };
    }

    /// <summary>
    /// The table as it is shown, without its groups, widgets and notes.
    /// </summary>
    public SheetTable MapTableHeader(DbSheetTable table)
    {
        var revision = table.Revisions.Shown();
        return new SheetTable
        {
            Id = table.Id,
            SheetId = table.SheetId,
            TableTemplateId = table.TableTemplateId,
            ReferenceId = table.ReferenceId,
            CreatedDateTime = table.CreatedDateTime,
            Revision = MapRevision(revision),
            Title = revision.Title,
            DisplayOrder = revision.DisplayOrder,
            ConfigurationOverride = revision.ConfigurationOverride,
            Groups = [],
            Widgets = [],
            Notes = []
        };
    }

    private SheetTable MapTable(DbSheetTable table)
    {
        var revision = table.Revisions.Shown();
        return new SheetTable
        {
            Id = table.Id,
            SheetId = table.SheetId,
            TableTemplateId = table.TableTemplateId,
            ReferenceId = table.ReferenceId,
            CreatedDateTime = table.CreatedDateTime,
            Revision = MapRevision(revision),
            Title = revision.Title,
            DisplayOrder = revision.DisplayOrder,
            ConfigurationOverride = revision.ConfigurationOverride,
            Groups = [.. table.Groups
                .Where(group => group.Revisions.IsShown())
                .Select(MapGroup)
                .OrderBy(group => group.DisplayOrder)
                .ThenBy(group => group.Id)],
            Widgets = [.. revision.WidgetValues.OrderBy(value => value.SheetTableWidgetId).Select(MapTableWidget)],
            Notes = [.. revision.NoteValues.Select(MapNote).OrderBy(note => note.CreatedDateTime).ThenBy(note => note.Id)]
        };
    }

    private static SheetTableWidget MapTableWidget(DbSheetTableWidgetValue value)
    {
        var widget = value.SheetTableWidget!;
        return new SheetTableWidget
        {
            Id = widget.Id,
            SheetTableId = widget.SheetTableId,
            TableTemplateWidgetId = widget.TableTemplateWidgetId,
            Value = value.Value
        };
    }

    private SheetTableNote MapNote(DbSheetTableNoteValue value)
    {
        var note = value.SheetTableNote!;
        return new SheetTableNote
        {
            Id = note.Id,
            SheetTableId = note.SheetTableId,
            Note = value.Note,
            IsObsolete = value.IsObsolete,
            SourceSystem = note.SourceSystem,
            CreatedByUserId = note.CreatedByUserId,
            CreatedDateTime = note.CreatedDateTime,
            CreatedByUser = User(note.CreatedByUserId)
        };
    }

    private SheetTableGroup MapGroup(DbSheetTableGroup group)
    {
        var revision = group.Revisions.Shown();
        return new SheetTableGroup
        {
            Id = group.Id,
            SheetTableId = group.SheetTableId,
            TableGroupTemplateId = group.TableGroupTemplateId,
            ReferenceId = group.ReferenceId,
            CreatedDateTime = group.CreatedDateTime,
            Revision = MapRevision(revision),
            DisplayOrder = revision.DisplayOrder,
            LinkedGroupReferenceId = revision.LinkedSheetTableGroupReferenceId,
            Rows = [.. group.Rows
                .Where(row => row.Revisions.IsShown())
                .Select(MapRow)
                .OrderBy(row => row.DisplayOrder)
                .ThenBy(row => row.Id)],
            Widgets = [.. revision.WidgetValues.OrderBy(value => value.SheetTableGroupWidgetId).Select(MapGroupWidget)]
        };
    }

    private static SheetTableGroupWidget MapGroupWidget(DbSheetTableGroupWidgetValue value)
    {
        var widget = value.SheetTableGroupWidget!;
        return new SheetTableGroupWidget
        {
            Id = widget.Id,
            SheetTableGroupId = widget.SheetTableGroupId,
            TableGroupTemplateWidgetId = widget.TableGroupTemplateWidgetId,
            Value = value.Value
        };
    }

    private SheetTableRow MapRow(DbSheetTableRow row)
    {
        var revision = row.Revisions.Shown();
        var values = new RowValues(revision);
        return new SheetTableRow
        {
            Id = row.Id,
            SheetTableGroupId = row.SheetTableGroupId,
            TableRowTemplateId = row.TableRowTemplateId,
            ReferenceId = row.ReferenceId,
            CreatedDateTime = row.CreatedDateTime,
            Revision = MapRevision(revision),
            DisplayOrder = revision.DisplayOrder,
            Cells = [.. row.Cells
                .OrderBy(cell => cell.TableCellTemplate!.ColumnNumber)
                .Select(cell => MapCell(cell, values))]
        };
    }

    private SheetTableCell MapCell(DbSheetTableCell cell, RowValues values)
    {
        var templateCell = cell.TableCellTemplate!;
        return new SheetTableCell
        {
            Id = cell.Id,
            SheetTableRowId = cell.SheetTableRowId,
            TableCellTemplateId = cell.TableCellTemplateId,
            TableCellTypeId = templateCell.TableCellTypeId,
            CellConfigurationId = templateCell.CellConfigurationId,
            ColumnNumber = templateCell.ColumnNumber,
            ColSpan = templateCell.ColSpan,
            RowSpan = templateCell.RowSpan,
            ReferenceId = cell.ReferenceId,
            CreatedDateTime = cell.CreatedDateTime,
            Value = values.ValueOf(cell.Id, templateCell.TableCellType!.ModelType),
            ConfigurationOverride = values.ConfigurationOverrideOf(cell.Id),
            CellConfiguration = templateCell.CellConfiguration is null ? null : MapOnce(_cellConfigurations, templateCell.CellConfigurationId, () => templateCell.CellConfiguration.MapToDomain()),
            TableCellType = MapOnce(_cellTypes, templateCell.TableCellTypeId, () => templateCell.TableCellType!.MapToDomain())
        };
    }

    // Template cells share a few cell types and configurations, so each is mapped once for the sheet, not for every cell.
    private static TMapped MapOnce<TMapped>(Dictionary<int, TMapped> mapped, int id, Func<TMapped> map)
    {
        if (!mapped.TryGetValue(id, out var value))
        {
            value = map();
            mapped[id] = value;
        }

        return value;
    }

    /// <summary>
    /// The values of one row revision by cell, so that each cell finds its value without searching the revision.
    /// </summary>
    private sealed class RowValues
    {
        private readonly Dictionary<int, DbSheetNumericValue> _numbers;
        private readonly Dictionary<int, DbSheetTextValue> _texts;
        private readonly Dictionary<int, DbSheetPairValue> _pairs;
        private readonly Dictionary<int, DbSheetLinkedSheetValue> _links;
        private readonly Dictionary<int, DbSheetCellConfigurationOverride> _overrides;

        public RowValues(DbSheetTableRowRevision revision)
        {
            _numbers = revision.NumericValues.ToDictionary(value => value.SheetTableCellId);
            _texts = revision.TextValues.ToDictionary(value => value.SheetTableCellId);
            _pairs = revision.PairValues.ToDictionary(value => value.SheetTableCellId);
            _links = revision.LinkedSheetValues.ToDictionary(value => value.SheetTableCellId);
            _overrides = revision.CellConfigurationOverrides.ToDictionary(value => value.SheetTableCellId);
        }

        public ICellProperties? ConfigurationOverrideOf(int cellId)
        {
            return _overrides.GetValueOrDefault(cellId)?.ConfigurationOverride;
        }

        // The value the row revision has for the cell. A cell the revision has no value for is empty, of the kind its type holds.
        public SheetCellValue ValueOf(int cellId, string modelType)
        {
            if (_numbers.TryGetValue(cellId, out var number))
            {
                return new SheetNumericValue { SheetTableCellId = cellId, Value = number.Value };
            }

            if (_texts.TryGetValue(cellId, out var text))
            {
                return new SheetTextValue { SheetTableCellId = cellId, Value = text.Value };
            }

            if (_pairs.TryGetValue(cellId, out var pair))
            {
                return new SheetPairValue { SheetTableCellId = cellId, ValueX = pair.PairValueX, ValueY = pair.PairValueY };
            }

            if (_links.TryGetValue(cellId, out var link))
            {
                return new SheetLinkedSheetValue
                {
                    SheetTableCellId = cellId,
                    TableReferenceId = link.TableReferenceId,
                    LinkedSpecSheetTypeId = link.LinkedSpecSheetTypeId,
                    LinkedSpecSheetPhaseId = link.LinkedSpecSheetPhaseId,
                    LinkedReferenceId = link.LinkedReferenceId
                };
            }

            return modelType switch
            {
                "numeric" or "numericDropdown" => new SheetNumericValue { SheetTableCellId = cellId, Value = null },
                "pair" => new SheetPairValue { SheetTableCellId = cellId, ValueX = null, ValueY = null },
                "linkedSpecSheet" => new SheetLinkedSheetValue { SheetTableCellId = cellId, TableReferenceId = null, LinkedSpecSheetTypeId = null, LinkedSpecSheetPhaseId = null, LinkedReferenceId = null },
                _ => new SheetTextValue { SheetTableCellId = cellId, Value = null }
            };
        }
    }

    private SheetProperty MapProperty(DbSheetProperty property)
    {
        var revision = property.Revisions.Shown();
        return new SheetProperty
        {
            Id = property.Id,
            SheetId = property.SheetId,
            SpecSheetTypePropertyId = property.SpecSheetTypePropertyId,
            CreatedDateTime = property.CreatedDateTime,
            Revision = MapRevision(revision),
            Value = revision.PropertyValue,
            SpecSheetTypeProperty = property.SpecSheetTypeProperty?.MapToDomain()
        };
    }

    private SheetLinkedTable MapLinkedTable(DbSheetLinkedTable linked)
    {
        var revision = linked.Revisions.Shown();
        return new SheetLinkedTable
        {
            Id = linked.Id,
            SheetId = linked.SheetId,
            TableReferenceId = linked.TableReferenceId,
            LinkedByUserId = linked.LinkedByUserId,
            LinkedByUser = User(linked.LinkedByUserId),
            LinkedDateTime = linked.LinkedDateTime,
            CreatedDateTime = linked.CreatedDateTime,
            Revision = MapRevision(revision),
            DisplayOrder = revision.DisplayOrder
        };
    }

    private static SheetExternalReference MapExternalReference(DbSheetExternalReference reference)
    {
        var revision = reference.Revisions.Single();
        return new SheetExternalReference
        {
            Id = reference.Id,
            SheetId = reference.SheetId,
            ReferenceId = reference.ReferenceId,
            CreatedDateTime = reference.CreatedDateTime,
            RevisionNumber = revision.RevisionNumber,
            SheetVersionId = revision.SheetVersionId,
            PublishedDateTime = revision.PublishedDateTime,
            SupersededDateTime = revision.SupersededDateTime,
            ExternalId = revision.ExternalId,
            SystemName = revision.SystemName,
            OwnerName = revision.OwnerName,
            Url = revision.Url,
            Type = revision.Type,
            Grouping = revision.Grouping,
            SystemIconUrl = revision.SystemIconUrl,
            Description = revision.Description
        };
    }
}
