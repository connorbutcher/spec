using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// Makes the revisions the builder writes, each stamped from the <see cref="WriteContext"/> of the copy it is for, and
/// puts a row revision's cell values on it.
/// </summary>
internal sealed class RevisionFactory
{
    private readonly DbSheet _sheet;

    public RevisionFactory(DbSheet sheet)
    {
        _sheet = sheet;
    }

    public DbSheetTableRevision TableRevision(
        WriteContext context,
        int revisionNumber,
        string title,
        int displayOrder,
        ITableProperties? configurationOverride,
        IEnumerable<DbSheetTableWidgetValue> widgetValues,
        IEnumerable<DbSheetTableNoteValue> noteValues,
        bool isDeleted)
    {
        return new DbSheetTableRevision
        {
            Id = 0,
            SheetTableId = 0,
            SheetId = _sheet.Id,
            RevisionNumber = revisionNumber,
            Status = context.Status,
            Title = title,
            DisplayOrder = displayOrder,
            ConfigurationOverride = configurationOverride,
            WidgetValues = [.. widgetValues],
            NoteValues = [.. noteValues],
            IsDeleted = isDeleted,
            CreatedByUserId = context.UserId,
            CreatedDateTime = context.At,
            LastModifiedDateTime = context.At,
            PublishedDateTime = context.PublishedDateTime,
            SupersededDateTime = null,
            SheetVersionId = null,
            SheetSubmissionId = null,
            SheetVersion = context.Version
        };
    }

    public DbSheetTableGroupRevision GroupRevision(
        WriteContext context,
        int revisionNumber,
        int displayOrder,
        Guid? linkedGroupReferenceId,
        IEnumerable<DbSheetTableGroupWidgetValue> widgetValues,
        bool isDeleted)
    {
        return new DbSheetTableGroupRevision
        {
            Id = 0,
            SheetTableGroupId = 0,
            SheetId = _sheet.Id,
            RevisionNumber = revisionNumber,
            Status = context.Status,
            DisplayOrder = displayOrder,
            LinkedSheetTableGroupReferenceId = linkedGroupReferenceId,
            WidgetValues = [.. widgetValues],
            IsDeleted = isDeleted,
            CreatedByUserId = context.UserId,
            CreatedDateTime = context.At,
            LastModifiedDateTime = context.At,
            PublishedDateTime = context.PublishedDateTime,
            SupersededDateTime = null,
            SheetVersionId = null,
            SheetSubmissionId = null,
            SheetVersion = context.Version
        };
    }

    public DbSheetTableRowRevision RowRevision(WriteContext context, int revisionNumber, int displayOrder, bool isDeleted)
    {
        return new DbSheetTableRowRevision
        {
            Id = 0,
            SheetTableRowId = 0,
            SheetId = _sheet.Id,
            RevisionNumber = revisionNumber,
            Status = context.Status,
            DisplayOrder = displayOrder,
            IsDeleted = isDeleted,
            CreatedByUserId = context.UserId,
            CreatedDateTime = context.At,
            LastModifiedDateTime = context.At,
            PublishedDateTime = context.PublishedDateTime,
            SupersededDateTime = null,
            SheetVersionId = null,
            SheetSubmissionId = null,
            SheetVersion = context.Version
        };
    }

    public DbSheetPropertyRevision PropertyRevision(WriteContext context, int revisionNumber, string? value, bool isDeleted)
    {
        return new DbSheetPropertyRevision
        {
            Id = 0,
            SheetPropertyId = 0,
            SheetId = _sheet.Id,
            RevisionNumber = revisionNumber,
            Status = context.Status,
            PropertyValue = value,
            IsDeleted = isDeleted,
            CreatedByUserId = context.UserId,
            CreatedDateTime = context.At,
            LastModifiedDateTime = context.At,
            PublishedDateTime = context.PublishedDateTime,
            SupersededDateTime = null,
            SheetVersionId = null,
            SheetSubmissionId = null,
            SheetVersion = context.Version
        };
    }

    public DbSheetLinkedTableRevision LinkedTableRevision(WriteContext context, int revisionNumber, int displayOrder, bool isDeleted)
    {
        return new DbSheetLinkedTableRevision
        {
            Id = 0,
            SheetLinkedTableId = 0,
            SheetId = _sheet.Id,
            RevisionNumber = revisionNumber,
            Status = context.Status,
            DisplayOrder = displayOrder,
            IsDeleted = isDeleted,
            CreatedByUserId = context.UserId,
            CreatedDateTime = context.At,
            LastModifiedDateTime = context.At,
            PublishedDateTime = context.PublishedDateTime,
            SupersededDateTime = null,
            SheetVersionId = null,
            SheetSubmissionId = null,
            SheetVersion = context.Version
        };
    }

    public static void AddValues(DbSheetTableRowRevision revision, IReadOnlyList<(DbSheetTableCell Cell, CellContent Content)> contents)
    {
        foreach (var (cell, content) in contents)
        {
            if (content.Text is { } text)
            {
                revision.TextValues.Add(new DbSheetTextValue
                {
                    SheetTableRowRevisionId = 0,
                    SheetTableCellId = 0,
                    Value = text,
                    SheetTableCell = cell
                });
            }

            if (content.Number is { } number)
            {
                revision.NumericValues.Add(new DbSheetNumericValue
                {
                    SheetTableRowRevisionId = 0,
                    SheetTableCellId = 0,
                    Value = number,
                    SheetTableCell = cell
                });
            }

            if (content.HasPair)
            {
                revision.PairValues.Add(new DbSheetPairValue
                {
                    SheetTableRowRevisionId = 0,
                    SheetTableCellId = 0,
                    PairValueX = content.PairX,
                    PairValueY = content.PairY,
                    SheetTableCell = cell
                });
            }

            if (content.Linked is { } linked)
            {
                revision.LinkedSheetValues.Add(new DbSheetLinkedSheetValue
                {
                    SheetTableRowRevisionId = 0,
                    SheetTableCellId = 0,
                    LinkedSpecSheetTypeId = linked.SheetTypeId,
                    LinkedSpecSheetPhaseId = linked.PhaseId,
                    TableReferenceId = linked.TableReferenceId,
                    LinkedReferenceId = linked.ItemReferenceId,
                    SheetTableCell = cell
                });
            }

            if (content.ConfigurationOverride is { } configurationOverride)
            {
                revision.CellConfigurationOverrides.Add(new DbSheetCellConfigurationOverride
                {
                    SheetTableRowRevisionId = 0,
                    SheetTableCellId = 0,
                    ConfigurationOverride = configurationOverride,
                    SheetTableCell = cell
                });
            }
        }
    }
}
