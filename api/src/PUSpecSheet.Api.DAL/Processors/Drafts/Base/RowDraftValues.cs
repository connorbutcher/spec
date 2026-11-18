using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Base;

/// <summary>
/// Sets the values of a row draft's cells. A cell without a value has no value row, as the migrator writes them.
/// </summary>
internal class RowDraftValues
{
    private readonly PUSpecSheetDbContext _dbContext;

    public RowDraftValues(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    public DbSheetPairValue? PairOf(DbSheetTableRowRevision draft, int cellId)
    {
        return draft.PairValues.FirstOrDefault(v => v.SheetTableCellId == cellId);
    }

    public DbSheetLinkedSheetValue? LinkedOf(DbSheetTableRowRevision draft, int cellId)
    {
        return draft.LinkedSheetValues.FirstOrDefault(v => v.SheetTableCellId == cellId);
    }

    public void SetText(DbSheetTableRowRevision draft, int cellId, string? value)
    {
        var existing = draft.SheetTextValues.FirstOrDefault(v => v.SheetTableCellId == cellId);
        if (value is null)
        {
            Remove(draft.SheetTextValues, existing);
            return;
        }

        if (existing is null)
        {
            draft.SheetTextValues.Add(new DbSheetTextValue { SheetTableRowRevisionId = draft.Id, SheetTableCellId = cellId, Value = value });
            return;
        }

        _dbContext.Entry(existing).Property(v => v.Value).CurrentValue = value;
    }

    public void SetNumber(DbSheetTableRowRevision draft, int cellId, double? value)
    {
        var existing = draft.SheetNumericValues.FirstOrDefault(v => v.SheetTableCellId == cellId);
        if (!value.HasValue)
        {
            Remove(draft.SheetNumericValues, existing);
            return;
        }

        if (existing is null)
        {
            draft.SheetNumericValues.Add(new DbSheetNumericValue { SheetTableRowRevisionId = draft.Id, SheetTableCellId = cellId, Value = value.Value });
            return;
        }

        _dbContext.Entry(existing).Property(v => v.Value).CurrentValue = value.Value;
    }

    public void SetPair(DbSheetTableRowRevision draft, int cellId, string? valueX, string? valueY)
    {
        var existing = PairOf(draft, cellId);
        if (valueX is null && valueY is null)
        {
            Remove(draft.PairValues, existing);
            return;
        }

        if (existing is null)
        {
            draft.PairValues.Add(new DbSheetPairValue
            {
                SheetTableRowRevisionId = draft.Id,
                SheetTableCellId = cellId,
                PairValueX = valueX,
                PairValueY = valueY
            });
            return;
        }

        var entry = _dbContext.Entry(existing);
        entry.Property(v => v.PairValueX).CurrentValue = valueX;
        entry.Property(v => v.PairValueY).CurrentValue = valueY;
    }

    public void SetLinked(DbSheetTableRowRevision draft, int cellId, LinkedDraftValue value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        var existing = LinkedOf(draft, cellId);
        if (value.IsEmpty)
        {
            Remove(draft.LinkedSheetValues, existing);
            return;
        }

        if (existing is null)
        {
            draft.LinkedSheetValues.Add(new DbSheetLinkedSheetValue
            {
                SheetTableRowRevisionId = draft.Id,
                SheetTableCellId = cellId,
                LinkedSpecSheetTypeId = value.LinkedSpecSheetTypeId,
                LinkedSpecSheetPhaseId = value.LinkedSpecSheetPhaseId,
                TableReferenceId = value.TableReferenceId,
                LinkedReferenceId = value.LinkedReferenceId
            });
            return;
        }

        var entry = _dbContext.Entry(existing);
        entry.Property(v => v.LinkedSpecSheetTypeId).CurrentValue = value.LinkedSpecSheetTypeId;
        entry.Property(v => v.LinkedSpecSheetPhaseId).CurrentValue = value.LinkedSpecSheetPhaseId;
        entry.Property(v => v.TableReferenceId).CurrentValue = value.TableReferenceId;
        entry.Property(v => v.LinkedReferenceId).CurrentValue = value.LinkedReferenceId;
    }

    /// <summary>
    /// Sets the cell's value from a cell value of any kind.
    /// </summary>
    public void SetValue(DbSheetTableRowRevision draft, int cellId, SheetCellValue? value)
    {
        switch (value)
        {
            case SheetTextValue text:
                SetText(draft, cellId, text.Value);
                break;
            case SheetNumericValue number:
                SetNumber(draft, cellId, number.Value);
                break;
            case SheetPairValue pair:
                SetPair(draft, cellId, pair.ValueX, pair.ValueY);
                break;
            case SheetLinkedSheetValue linked:
                SetLinked(draft, cellId, new LinkedDraftValue
                {
                    LinkedSpecSheetTypeId = linked.LinkedSpecSheetTypeId,
                    LinkedSpecSheetPhaseId = linked.LinkedSpecSheetPhaseId,
                    TableReferenceId = linked.TableReferenceId,
                    LinkedReferenceId = linked.LinkedReferenceId
                });
                break;
        }
    }

    /// <summary>
    /// Sets the cell's value from a cell value of any kind, and removes any value of another kind the cell holds.
    /// </summary>
    public void ReplaceValue(DbSheetTableRowRevision draft, int cellId, SheetCellValue? value)
    {
        if (value is not SheetTextValue)
        {
            SetText(draft, cellId, null);
        }

        if (value is not SheetNumericValue)
        {
            SetNumber(draft, cellId, null);
        }

        if (value is not SheetPairValue)
        {
            SetPair(draft, cellId, null, null);
        }

        if (value is not SheetLinkedSheetValue)
        {
            Remove(draft.LinkedSheetValues, LinkedOf(draft, cellId));
        }

        SetValue(draft, cellId, value);
    }

    /// <summary>
    /// Gives a cell added in the draft its value, before the cell has an id.
    /// </summary>
    public void AddValue(DbSheetTableRowRevision draft, DbSheetTableCell cell, SheetCellValue? value)
    {
        switch (value)
        {
            case SheetTextValue { Value: { } text }:
                draft.SheetTextValues.Add(new DbSheetTextValue { SheetTableRowRevisionId = draft.Id, SheetTableCellId = cell.Id, Value = text, SheetTableCell = cell });
                break;
            case SheetNumericValue { Value: { } number }:
                draft.SheetNumericValues.Add(new DbSheetNumericValue { SheetTableRowRevisionId = draft.Id, SheetTableCellId = cell.Id, Value = number, SheetTableCell = cell });
                break;
            case SheetPairValue pair when pair.ValueX is not null || pair.ValueY is not null:
                draft.PairValues.Add(new DbSheetPairValue
                {
                    SheetTableRowRevisionId = draft.Id,
                    SheetTableCellId = cell.Id,
                    PairValueX = pair.ValueX,
                    PairValueY = pair.ValueY,
                    SheetTableCell = cell
                });
                break;
            case SheetLinkedSheetValue linked when linked.LinkedSpecSheetTypeId.HasValue
                || linked.LinkedSpecSheetPhaseId.HasValue
                || linked.TableReferenceId.HasValue
                || linked.LinkedReferenceId.HasValue:
                draft.LinkedSheetValues.Add(new DbSheetLinkedSheetValue
                {
                    SheetTableRowRevisionId = draft.Id,
                    SheetTableCellId = cell.Id,
                    LinkedSpecSheetTypeId = linked.LinkedSpecSheetTypeId,
                    LinkedSpecSheetPhaseId = linked.LinkedSpecSheetPhaseId,
                    TableReferenceId = linked.TableReferenceId,
                    LinkedReferenceId = linked.LinkedReferenceId,
                    SheetTableCell = cell
                });
                break;
        }
    }

    public void SetConfigurationOverride(DbSheetTableRowRevision draft, int cellId, ICellProperties? configurationOverride)
    {
        var existing = draft.CellConfigurationOverrides.FirstOrDefault(v => v.SheetTableCellId == cellId);
        if (configurationOverride is null)
        {
            Remove(draft.CellConfigurationOverrides, existing);
            return;
        }

        if (existing is null)
        {
            draft.CellConfigurationOverrides.Add(new DbSheetCellConfigurationOverride
            {
                SheetTableRowRevisionId = draft.Id,
                SheetTableCellId = cellId,
                ConfigurationOverride = configurationOverride
            });
            return;
        }

        _dbContext.Entry(existing).Property(v => v.ConfigurationOverride).CurrentValue = configurationOverride;
    }

    /// <summary>
    /// Gives a cell added in the draft its configuration override, before the cell has an id.
    /// </summary>
    public void AddConfigurationOverride(DbSheetTableRowRevision draft, DbSheetTableCell cell, ICellProperties configurationOverride)
    {
        draft.CellConfigurationOverrides.Add(new DbSheetCellConfigurationOverride
        {
            SheetTableRowRevisionId = draft.Id,
            SheetTableCellId = cell.Id,
            ConfigurationOverride = configurationOverride,
            SheetTableCell = cell
        });
    }

    /// <summary>
    /// Removes everything the draft holds for the cell: its values and configuration override.
    /// </summary>
    public void Clear(DbSheetTableRowRevision draft, int cellId)
    {
        SetText(draft, cellId, null);
        SetNumber(draft, cellId, null);
        SetPair(draft, cellId, null, null);
        Remove(draft.LinkedSheetValues, LinkedOf(draft, cellId));
        SetConfigurationOverride(draft, cellId, null);
    }

    private void Remove<TValue>(ICollection<TValue> values, TValue? existing)
        where TValue : class
    {
        if (existing is null)
        {
            return;
        }

        values.Remove(existing);
        _dbContext.Remove(existing);
    }
}