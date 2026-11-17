using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.SpecSheets;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.References;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Values;
using PUSpecSheet.Api.Database.Schema.Workflow;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

/// <summary>
/// Reads a published spec sheet from the revisions: the version that was migrated from the SpecSheets copy with the
/// given id, as its sheet stood when the version was published. It builds the same <see cref="DbSpecSheet"/> a copy is
/// read into and maps it the same way, so everything after the read is the same whichever it came from.
/// </summary>
/// <remarks>
/// Tables, groups, rows and cells have the ids of their spec items; widgets, notes and workflow step actors, which
/// the revisions keep inside their items, are numbered from 1 within the sheet. Who created and last changed an item
/// is who wrote its first revision and the one shown, as the revisions don't keep it per item.
/// </remarks>
internal class SpecSheetRevisionReader
{
    private readonly PUSpecSheetDbContext _dbContext;

    public SpecSheetRevisionReader(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    /// <summary>
    /// The published spec sheet, or null when the copy with this id hasn't been migrated to a version.
    /// </summary>
    public async Task<SpecSheet?> ReadPublishedAsync(int specSheetId, CancellationToken cancellationToken)
    {
        var version = await _dbContext.SheetVersions
            .AsNoTracking()
            .Where(v => v.SourceSpecSheetId == specSheetId)
            .OrderByDescending(v => v.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (version is null)
        {
            return null;
        }

        var sheet = await _dbContext.Sheets
            .AsNoTracking()
            .Where(s => s.Id == version.SheetId)
            .Select(s => new { s.SpecSheetPhaseId, s.SpecSheetTypeId })
            .FirstAsync(cancellationToken);
        var moment = version.PublishedDateTime;
        var ids = new Numbering();

        // The users are few, and notes and approvals can name any of them.
        var users = await _dbContext.Users
            .AsNoTracking()
            .ToDictionaryAsync(user => user.Id, cancellationToken);

        var tables = await ReadTablesAsync(specSheetId, version.SheetId, moment, ids, users, cancellationToken);
        var propertyValues = await ReadPropertyValuesAsync(specSheetId, version.SheetId, moment, cancellationToken);
        var externalReferences = await ReadExternalReferencesAsync(specSheetId, version.SheetId, moment, cancellationToken);
        var workflowSteps = await ReadWorkflowStepsAsync(specSheetId, version, ids, users, cancellationToken);
        var createdByUserId = version.SourceCreatedByUserId ?? version.PublishedByUserId;

        var dbSpecSheet = new DbSpecSheet
        {
            Id = specSheetId,
            LegacySpecSheetId = version.LegacySpecSheetId,
            SpecSheetPhaseId = sheet.SpecSheetPhaseId,
            SpecSheetTypeId = sheet.SpecSheetTypeId,
            IsPublished = true,
            CreatedByUserId = createdByUserId,
            CreatedDateTime = version.SourceCreatedDateTime ?? version.PublishedDateTime,
            HasBeenMigrated = version.HasBeenMigrated,
            CreatedByUser = users.GetValueOrDefault(createdByUserId),
            PublishedByUserId = version.PublishedByUserId,
            PublishedDateTime = version.PublishedDateTime,
            ChangeLog = version.ChangeLog,
            PublishedByUser = users.GetValueOrDefault(version.PublishedByUserId),
            SpecSheetPhase = await _dbContext.SpecSheetPhases.AsNoTracking().FirstAsync(phase => phase.Id == sheet.SpecSheetPhaseId, cancellationToken),
            SpecSheetType = await _dbContext.SpecSheetTypes.AsNoTracking().FirstAsync(type => type.Id == sheet.SpecSheetTypeId, cancellationToken),
            Tables = tables,
            PropertyValues = propertyValues,
            WorkflowSteps = workflowSteps,
            ExternalReferences = externalReferences
        };

        return dbSpecSheet.MapToDomain();
    }

    private async Task<List<DbSpecSheetTable>> ReadTablesAsync(
        int specSheetId,
        int sheetId,
        DateTime moment,
        Numbering ids,
        Dictionary<int, DbUser> users,
        CancellationToken cancellationToken)
    {
        var tableRevisions = await _dbContext.SheetTableRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == sheetId && !revision.IsDeleted)
            .PublishedAt(moment)
            .ToListAsync(cancellationToken);
        var groupRevisions = await _dbContext.SheetTableGroupRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == sheetId && !revision.IsDeleted)
            .PublishedAt(moment)
            .ToListAsync(cancellationToken);
        var rowRevisions = await _dbContext.SheetTableRowRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == sheetId && !revision.IsDeleted)
            .PublishedAt(moment)
            .ToListAsync(cancellationToken);

        var tableItems = await _dbContext.SheetTables
            .AsNoTracking()
            .Where(table => table.SheetId == sheetId)
            .ToDictionaryAsync(table => table.Id, cancellationToken);
        var sheetTables = _dbContext.SheetTables.Where(table => table.SheetId == sheetId);
        var groupItems = await _dbContext.SheetTableGroups
            .AsNoTracking()
            .Where(group => sheetTables.Any(table => table.Id == group.SheetTableId))
            .ToDictionaryAsync(group => group.Id, cancellationToken);
        var sheetGroups = _dbContext.SheetTableGroups.Where(group => sheetTables.Any(table => table.Id == group.SheetTableId));
        var rowItems = await _dbContext.SheetTableRows
            .AsNoTracking()
            .Where(row => sheetGroups.Any(group => group.Id == row.SheetTableGroupId))
            .ToDictionaryAsync(row => row.Id, cancellationToken);
        var firstAuthors = await FirstAuthorsAsync(sheetId, cancellationToken);

        // What is on the sheet: shown, and in a table or group that is shown.
        var shownTables = tableRevisions.ToDictionary(revision => revision.SheetTableId);
        var shownGroups = groupRevisions
            .Where(revision => shownTables.ContainsKey(groupItems[revision.SheetTableGroupId].SheetTableId))
            .ToDictionary(revision => revision.SheetTableGroupId);
        var shownRows = rowRevisions
            .Where(revision => shownGroups.ContainsKey(rowItems[revision.SheetTableRowId].SheetTableGroupId))
            .OrderBy(revision => revision.DisplayOrder)
            .ToList();

        var cells = await ReadCellsAsync(sheetId, shownRows, firstAuthors, cancellationToken);

        // A group is nested under the group it is linked to, which the revisions keep by ReferenceID.
        var groupIdsByReference = shownGroups.Keys
            .GroupBy(groupId => (groupItems[groupId].SheetTableId, groupItems[groupId].ReferenceId))
            .ToDictionary(group => group.Key, group => group.First());

        var rows = shownRows
            .Select(revision =>
            {
                var item = rowItems[revision.SheetTableRowId];
                var row = new DbSpecSheetTableRow
                {
                    Id = item.Id,
                    SpecSheetTableGroupId = item.SheetTableGroupId,
                    TableRowTemplateId = item.TableRowTemplateId,
                    RowNumber = revision.DisplayOrder,
                    ReferenceId = item.ReferenceId,
                    IsDeleted = false,
                    CreatedByUserId = firstAuthors.GetValueOrDefault(("row", item.Id), revision.CreatedByUserId),
                    CreatedDateTime = item.CreatedDateTime,
                    LastModifiedByUserId = revision.RevisionNumber > 1 ? revision.CreatedByUserId : null,
                    LastModifiedDateTime = revision.RevisionNumber > 1 ? revision.PublishedDateTime : null
                };
                foreach (var cell in cells[item.Id])
                {
                    cell.RowNumber = row.RowNumber;
                    row.Cells.Add(cell);
                }

                return row;
            })
            .ToLookup(row => row.SpecSheetTableGroupId);

        var groups = shownGroups.Values
            .Select(revision =>
            {
                var item = groupItems[revision.SheetTableGroupId];
                int? linkedGroupId = revision.LinkedSheetTableGroupReferenceId is { } linkedReference
                    && groupIdsByReference.TryGetValue((item.SheetTableId, linkedReference), out var linked)
                        ? linked
                        : null;
                return new DbSpecSheetTableGroup
                {
                    Id = item.Id,
                    SpecSheetTableId = item.SheetTableId,
                    TableGroupTemplateId = item.TableGroupTemplateId,
                    LinkedSpecSheetTableGroupId = linkedGroupId,
                    Order = revision.DisplayOrder,
                    ReferenceId = item.ReferenceId,
                    IsDeleted = false,
                    Rows = [.. rows[item.Id]],
                    Widgets = [.. (revision.WidgetValues ?? []).Select(widget =>
                    {
                        var widgetId = ids.Next("group widget");
                        return new DbSpecSheetTableGroupWidget
                        {
                            Id = widgetId,
                            SpecSheetTableGroupId = item.Id,
                            TableGroupTemplateWidgetId = widget.TemplateWidgetId,
                            Value = new DbTableGroupWidgetValue
                            {
                                Id = widgetId,
                                SpecSheetTableGroupWidgetId = widgetId,
                                Value = widget.Value
                            }
                        };
                    })],
                    CreatedByUserId = firstAuthors.GetValueOrDefault(("group", item.Id), revision.CreatedByUserId),
                    CreatedDateTime = item.CreatedDateTime,
                    LastModifiedByUserId = revision.RevisionNumber > 1 ? revision.CreatedByUserId : null,
                    LastModifiedDateTime = revision.RevisionNumber > 1 ? revision.PublishedDateTime : null
                };
            })
            .ToLookup(group => group.SpecSheetTableId);

        return shownTables.Values
            .Select(revision =>
            {
                var item = tableItems[revision.SheetTableId];
                return new DbSpecSheetTable
                {
                    Id = item.Id,
                    SpecSheetId = specSheetId,
                    TableTemplateId = item.TableTemplateId,
                    Title = revision.Title,
                    DisplayOrder = revision.DisplayOrder,
                    ReferenceId = item.ReferenceId,
                    IsDeleted = false,
                    CreatedByUserId = firstAuthors.GetValueOrDefault(("table", item.Id), revision.CreatedByUserId),
                    CreatedDateTime = item.CreatedDateTime,
                    LastModifiedByUserId = revision.RevisionNumber > 1 ? revision.CreatedByUserId : null,
                    LastModifiedDateTime = revision.RevisionNumber > 1 ? revision.PublishedDateTime : null,
                    ConfigurationOverride = revision.ConfigurationOverride,
                    Groups = [.. groups[item.Id]],
                    Widgets = [.. (revision.WidgetValues ?? []).Select(widget =>
                    {
                        var widgetId = ids.Next("table widget");
                        return new DbSpecSheetTableWidget
                        {
                            Id = widgetId,
                            SpecSheetTableId = item.Id,
                            TableTemplateWidgetId = widget.TemplateWidgetId,
                            Value = new DbTableWidgetValue
                            {
                                Id = widgetId,
                                SpecSheetTableWidgetId = widgetId,
                                Value = widget.Value
                            }
                        };
                    })],
                    Notes = [.. (revision.Notes ?? []).Select(note => new DbSpecSheetTableNote
                    {
                        Id = ids.Next("note"),
                        SpecSheetTableId = item.Id,
                        Note = note.Note,
                        SourceSystem = note.SourceSystem,
                        IsObsolete = note.IsObsolete,
                        CreatedByUserId = note.CreatedByUserId,
                        CreatedDateTime = note.CreatedDateTime,
                        CreatedByUser = users.GetValueOrDefault(note.CreatedByUserId)
                    })]
                };
            })
            .OrderBy(table => table.DisplayOrder)
            .ToList();
    }

    // The cells of the rows shown, each with its row revision's values. A cell has its template cell's type,
    // configuration and place, and an empty value of its type's kind where the revision holds none, as a copied cell
    // always has a value row.
    private async Task<ILookup<int, DbSpecSheetTableCell>> ReadCellsAsync(
        int sheetId,
        List<DbSheetTableRowRevision> shownRows,
        Dictionary<(string Kind, int Id), int> firstAuthors,
        CancellationToken cancellationToken)
    {
        var shownRowIds = shownRows.Select(revision => revision.SheetTableRowId).ToHashSet();
        var revisionOfRow = shownRows.ToDictionary(revision => revision.SheetTableRowId);
        var shownRevisionIds = shownRows.Select(revision => revision.Id).ToHashSet();

        var sheetTables = _dbContext.SheetTables.Where(table => table.SheetId == sheetId);
        var sheetGroups = _dbContext.SheetTableGroups.Where(group => sheetTables.Any(table => table.Id == group.SheetTableId));
        var sheetRows = _dbContext.SheetTableRows.Where(row => sheetGroups.Any(group => group.Id == row.SheetTableGroupId));
        var sheetRowRevisions = _dbContext.SheetTableRowRevisions.Where(revision => revision.SheetId == sheetId);

        var cellItems = (await _dbContext.SheetTableCells
                .AsNoTracking()
                .Where(cell => sheetRows.Any(row => row.Id == cell.SheetTableRowId))
                .ToListAsync(cancellationToken))
            .Where(cell => shownRowIds.Contains(cell.SheetTableRowId))
            .ToList();

        var templateCellIds = cellItems.Select(cell => cell.TableCellTemplateId).Distinct().ToList();
        var templateCells = await _dbContext.TableCellTemplates
            .AsNoTracking()
            .Where(templateCell => templateCellIds.Contains(templateCell.Id))
            .ToDictionaryAsync(templateCell => templateCell.Id, cancellationToken);
        var cellTypeIds = templateCells.Values.Select(templateCell => templateCell.TableCellTypeId).Distinct().ToList();
        var cellTypes = await _dbContext.TableCellTypes
            .AsNoTracking()
            .Where(cellType => cellTypeIds.Contains(cellType.Id))
            .ToDictionaryAsync(cellType => cellType.Id, cancellationToken);
        var configurationIds = templateCells.Values.Select(templateCell => templateCell.CellConfigurationId).Distinct().ToList();
        var configurations = await _dbContext.CellConfigurations
            .AsNoTracking()
            .Where(configuration => configurationIds.Contains(configuration.Id))
            .ToDictionaryAsync(configuration => configuration.Id, cancellationToken);

        var texts = (await _dbContext.SheetTextValues.AsNoTracking()
                .Where(value => sheetRowRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
                .ToListAsync(cancellationToken))
            .Where(value => shownRevisionIds.Contains(value.SheetTableRowRevisionId))
            .ToDictionary(value => value.SheetTableCellId);
        var numbers = (await _dbContext.SheetNumericValues.AsNoTracking()
                .Where(value => sheetRowRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
                .ToListAsync(cancellationToken))
            .Where(value => shownRevisionIds.Contains(value.SheetTableRowRevisionId))
            .ToDictionary(value => value.SheetTableCellId);
        var pairs = (await _dbContext.SheetPairValues.AsNoTracking()
                .Where(value => sheetRowRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
                .ToListAsync(cancellationToken))
            .Where(value => shownRevisionIds.Contains(value.SheetTableRowRevisionId))
            .ToDictionary(value => value.SheetTableCellId);
        var links = (await _dbContext.SheetLinkedSheetValues.AsNoTracking()
                .Where(value => sheetRowRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
                .ToListAsync(cancellationToken))
            .Where(value => shownRevisionIds.Contains(value.SheetTableRowRevisionId))
            .ToDictionary(value => value.SheetTableCellId);
        var overrides = (await _dbContext.SheetCellConfigurationOverrides.AsNoTracking()
                .Where(value => sheetRowRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
                .ToListAsync(cancellationToken))
            .Where(value => shownRevisionIds.Contains(value.SheetTableRowRevisionId))
            .ToDictionary(value => value.SheetTableCellId);

        return cellItems
            .Select(item =>
            {
                var templateCell = templateCells[item.TableCellTemplateId];
                var cellType = cellTypes[templateCell.TableCellTypeId];
                var revision = revisionOfRow[item.SheetTableRowId];
                var cell = new DbSpecSheetTableCell
                {
                    Id = item.Id,
                    SpecSheetTableRowId = item.SheetTableRowId,
                    TableCellTypeId = templateCell.TableCellTypeId,
                    CellConfigurationId = templateCell.CellConfigurationId,
                    RowNumber = revision.DisplayOrder,
                    ColumnNumber = templateCell.ColumnNumber,
                    ColSpan = templateCell.ColSpan,
                    RowSpan = templateCell.RowSpan,
                    ReferenceId = item.ReferenceId,
                    IsDeleted = false,
                    CreatedByUserId = firstAuthors.GetValueOrDefault(("row", item.SheetTableRowId), revision.CreatedByUserId),
                    CreatedDateTime = item.CreatedDateTime,
                    LastModifiedByUserId = revision.RevisionNumber > 1 ? revision.CreatedByUserId : null,
                    LastModifiedDateTime = revision.RevisionNumber > 1 ? revision.PublishedDateTime : null,
                    ConfigurationOverride = overrides.GetValueOrDefault(item.Id)?.ConfigurationOverride,
                    TableCellType = cellType,
                    CellConfiguration = configurations[templateCell.CellConfigurationId]
                };

                if (numbers.TryGetValue(item.Id, out var number))
                {
                    cell.NumericValue = new DbNumericValue { Id = item.Id, SpecSheetTableCellId = item.Id, Value = number.Value };
                }
                else if (texts.TryGetValue(item.Id, out var text))
                {
                    cell.TextValue = new DbTextValue { Id = item.Id, SpecSheetTableCellId = item.Id, Value = text.Value };
                }
                else if (pairs.TryGetValue(item.Id, out var pair))
                {
                    cell.PairValue = new DbPairValue { Id = item.Id, SpecSheetTableCellId = item.Id, PairValueX = pair.PairValueX, PairValueY = pair.PairValueY };
                }
                else if (links.TryGetValue(item.Id, out var link))
                {
                    cell.LinkedSpecSheetValue = new DbLinkedSpecSheetValue
                    {
                        Id = item.Id,
                        SpecSheetTableCellId = item.Id,
                        TableReferenceId = link.TableReferenceId,
                        LinkedSpecSheetTypeId = link.LinkedSpecSheetTypeId,
                        LinkedSpecSheetPhaseId = link.LinkedSpecSheetPhaseId,
                        LinkedReferenceId = link.LinkedReferenceId
                    };
                }
                else
                {
                    SetEmptyValue(cell, cellType.ModelType);
                }

                return cell;
            })
            .OrderBy(cell => cell.ColumnNumber)
            .ToLookup(cell => cell.SpecSheetTableRowId);
    }

    // The kind of value a cell type holds, as the app gives a new cell one.
    private static void SetEmptyValue(DbSpecSheetTableCell cell, string modelType)
    {
        switch (modelType)
        {
            case "numeric":
            case "numericDropdown":
                cell.NumericValue = new DbNumericValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, Value = null };
                break;
            case "pair":
                cell.PairValue = new DbPairValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, PairValueX = null, PairValueY = null };
                break;
            case "linkedSpecSheet":
                cell.LinkedSpecSheetValue = new DbLinkedSpecSheetValue
                {
                    Id = cell.Id,
                    SpecSheetTableCellId = cell.Id,
                    TableReferenceId = null,
                    LinkedSpecSheetTypeId = null,
                    LinkedSpecSheetPhaseId = null,
                    LinkedReferenceId = null
                };
                break;
            default:
                cell.TextValue = new DbTextValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, Value = null };
                break;
        }
    }

    // Who wrote each table's, group's and row's first revision, by kind and item id.
    private async Task<Dictionary<(string Kind, int Id), int>> FirstAuthorsAsync(int sheetId, CancellationToken cancellationToken)
    {
        var authors = new Dictionary<(string Kind, int Id), int>();
        foreach (var first in await _dbContext.SheetTableRevisions.AsNoTracking()
            .Where(revision => revision.SheetId == sheetId && revision.RevisionNumber == 1)
            .Select(revision => new { revision.SheetTableId, revision.CreatedByUserId })
            .ToListAsync(cancellationToken))
        {
            authors[("table", first.SheetTableId)] = first.CreatedByUserId;
        }

        foreach (var first in await _dbContext.SheetTableGroupRevisions.AsNoTracking()
            .Where(revision => revision.SheetId == sheetId && revision.RevisionNumber == 1)
            .Select(revision => new { revision.SheetTableGroupId, revision.CreatedByUserId })
            .ToListAsync(cancellationToken))
        {
            authors[("group", first.SheetTableGroupId)] = first.CreatedByUserId;
        }

        foreach (var first in await _dbContext.SheetTableRowRevisions.AsNoTracking()
            .Where(revision => revision.SheetId == sheetId && revision.RevisionNumber == 1)
            .Select(revision => new { revision.SheetTableRowId, revision.CreatedByUserId })
            .ToListAsync(cancellationToken))
        {
            authors[("row", first.SheetTableRowId)] = first.CreatedByUserId;
        }

        return authors;
    }

    private async Task<List<DbSpecSheetPropertyValue>> ReadPropertyValuesAsync(int specSheetId, int sheetId, DateTime moment, CancellationToken cancellationToken)
    {
        var revisions = await _dbContext.SheetPropertyRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == sheetId && !revision.IsDeleted)
            .PublishedAt(moment)
            .ToListAsync(cancellationToken);
        var items = await _dbContext.SheetProperties
            .AsNoTracking()
            .Where(property => property.SheetId == sheetId)
            .ToDictionaryAsync(property => property.Id, cancellationToken);
        var typePropertyIds = items.Values.Select(property => property.SpecSheetTypePropertyId).Distinct().ToList();
        var typeProperties = await _dbContext.SpecSheetTypeProperties
            .AsNoTracking()
            .Where(property => typePropertyIds.Contains(property.Id))
            .ToDictionaryAsync(property => property.Id, cancellationToken);

        return revisions
            .Select(revision =>
            {
                var item = items[revision.SheetPropertyId];
                return new DbSpecSheetPropertyValue
                {
                    Id = item.Id,
                    SpecSheetId = specSheetId,
                    SpecSheetTypePropertyId = item.SpecSheetTypePropertyId,
                    PropertyValue = revision.PropertyValue,
                    SpecSheetTypeProperty = typeProperties[item.SpecSheetTypePropertyId]
                };
            })
            .ToList();
    }

    private async Task<List<DbSpecSheetExternalReference>> ReadExternalReferencesAsync(int specSheetId, int sheetId, DateTime moment, CancellationToken cancellationToken)
    {
        var items = await _dbContext.SheetExternalReferences
            .AsNoTracking()
            .Where(reference => reference.SheetId == sheetId)
            .ToDictionaryAsync(reference => reference.Id, cancellationToken);

        return (await _dbContext.SheetExternalReferenceRevisions
                .AsNoTracking()
                .Where(revision => revision.SheetId == sheetId
                    && !revision.IsDeleted
                    && revision.PublishedDateTime <= moment
                    && (revision.SupersededDateTime == null || revision.SupersededDateTime > moment))
                .ToListAsync(cancellationToken))
            .Select(revision => new DbSpecSheetExternalReference
            {
                Id = revision.SheetExternalReferenceId,
                SpecSheetId = specSheetId,
                ExternalId = revision.ExternalId,
                ReferenceId = items[revision.SheetExternalReferenceId].ReferenceId,
                SystemName = revision.SystemName,
                OwnerName = revision.OwnerName,
                Url = revision.Url,
                Type = revision.Type,
                Grouping = revision.Grouping,
                SystemIconUrl = revision.SystemIconUrl,
                Description = revision.Description
            })
            .ToList();
    }

    // A version's approval history: the submission that published it, and those decided (or overtaken) since the
    // version before it, as the copy's workflow steps were. A withdrawn submission was a step left pending.
    private async Task<List<DbWorkflowStep>> ReadWorkflowStepsAsync(
        int specSheetId,
        DbSheetVersion version,
        Numbering ids,
        Dictionary<int, DbUser> users,
        CancellationToken cancellationToken)
    {
        var previousPublished = await _dbContext.SheetVersions
            .AsNoTracking()
            .Where(v => v.SheetId == version.SheetId && v.VersionNumber < version.VersionNumber)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => (DateTime?)v.PublishedDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        var submissions = await _dbContext.SheetSubmissions
            .AsNoTracking()
            .Include(submission => submission.Approvers)
            .Where(submission => submission.SheetId == version.SheetId
                && (submission.SheetVersionId == version.Id
                    || (submission.SheetVersionId == null
                        && submission.SubmittedDateTime <= version.PublishedDateTime
                        && (previousPublished == null || submission.SubmittedDateTime > previousPublished))))
            .OrderBy(submission => submission.SubmittedDateTime)
            .ThenBy(submission => submission.Id)
            .ToListAsync(cancellationToken);

        return submissions
            .Select(submission => new DbWorkflowStep
            {
                Id = submission.Id,
                SpecSheetId = specSheetId,
                StartedDateTime = submission.SubmittedDateTime,
                State = submission.Status switch
                {
                    SubmissionStatus.Approved => WorkflowState.Approved,
                    SubmissionStatus.Rejected => WorkflowState.Rejected,
                    _ => WorkflowState.Pending
                },
                Actors = [.. submission.Approvers
                    .OrderBy(approver => approver.UserId)
                    .Select(approver => new DbWorkflowStepActor
                    {
                        Id = ids.Next("actor"),
                        WorkflowStepId = submission.Id,
                        ActorUserId = approver.UserId,
                        ActorUser = users.GetValueOrDefault(approver.UserId),
                        State = approver.Decision switch
                        {
                            ApproverDecision.Approved => WorkflowState.Approved,
                            ApproverDecision.Rejected => WorkflowState.Rejected,
                            _ => WorkflowState.Pending
                        },
                        Comment = approver.Reason == DbSheetSubmissionApprover.NoReasonGiven ? null : approver.Reason,
                        ActionedDateTime = approver.DecidedDateTime
                    })]
            })
            .ToList();
    }

    // Ids for what the revisions keep inside their items, numbered from 1 within the sheet for each kind.
    private sealed class Numbering
    {
        private readonly Dictionary<string, int> _last = [];

        public int Next(string kind)
        {
            _last[kind] = _last.GetValueOrDefault(kind) + 1;
            return _last[kind];
        }
    }
}
