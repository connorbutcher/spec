using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Mappers.Configuration;
using PUSpecSheet.Api.Database.Mappers.Template;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Schema.Access;
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
            HasBeenMigrated = version.HasBeenMigrated
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
                .Select(cell => MapCell(cell, revision))]
        };
    }

    private static SheetTableCell MapCell(DbSheetTableCell cell, DbSheetTableRowRevision revision)
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
            Value = MapValue(cell.Id, templateCell.TableCellType!.ModelType, revision),
            ConfigurationOverride = revision.CellConfigurationOverrides.FirstOrDefault(value => value.SheetTableCellId == cell.Id)?.ConfigurationOverride,
            CellConfiguration = templateCell.CellConfiguration?.MapToDomain(),
            TableCellType = templateCell.TableCellType.MapToDomain()
        };
    }

    // The value the row revision has for the cell. A cell the revision has no value for is empty, of the kind its type holds.
    private static SheetCellValue MapValue(int cellId, string modelType, DbSheetTableRowRevision revision)
    {
        if (revision.NumericValues.FirstOrDefault(value => value.SheetTableCellId == cellId) is { } number)
        {
            return new SheetNumericValue { SheetTableCellId = cellId, Value = number.Value };
        }

        if (revision.TextValues.FirstOrDefault(value => value.SheetTableCellId == cellId) is { } text)
        {
            return new SheetTextValue { SheetTableCellId = cellId, Value = text.Value };
        }

        if (revision.PairValues.FirstOrDefault(value => value.SheetTableCellId == cellId) is { } pair)
        {
            return new SheetPairValue { SheetTableCellId = cellId, ValueX = pair.PairValueX, ValueY = pair.PairValueY };
        }

        if (revision.LinkedSheetValues.FirstOrDefault(value => value.SheetTableCellId == cellId) is { } link)
        {
            return MapLinkedValue(cellId, link);
        }

        return modelType switch
        {
            "numeric" or "numericDropdown" => new SheetNumericValue { SheetTableCellId = cellId, Value = null },
            "pair" => new SheetPairValue { SheetTableCellId = cellId, ValueX = null, ValueY = null },
            "linkedSpecSheet" => new SheetLinkedSheetValue { SheetTableCellId = cellId, TableReferenceId = null, LinkedSpecSheetTypeId = null, LinkedSpecSheetPhaseId = null, LinkedReferenceId = null },
            _ => new SheetTextValue { SheetTableCellId = cellId, Value = null }
        };
    }

    private static SheetLinkedSheetValue MapLinkedValue(int cellId, DbSheetLinkedSheetValue link)
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
