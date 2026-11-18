using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// Reads the widget values of a sheet's copied tables and groups, by table or group.
/// </summary>
internal sealed class LegacyWidgetReader
{
    private readonly PUSpecSheetDbContext _dbContext;

    public LegacyWidgetReader(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    // A table's widget values, one per template widget; where a copy has more than one, the newest is kept.
    public async Task<Dictionary<int, IReadOnlyList<SnapshotWidgetValue>>> ReadTableWidgetsAsync(
        IQueryable<DbSpecSheetTable> tables,
        CancellationToken cancellationToken)
    {
        var widgets = _dbContext.SpecSheetTableWidgets.AsNoTracking()
            .Where(widget => tables.Any(table => table.Id == widget.SpecSheetTableId));
        var owners = await widgets
            .OrderBy(widget => widget.Id)
            .Select(widget => new { widget.Id, OwnerId = widget.SpecSheetTableId, TemplateWidgetId = widget.TableTemplateWidgetId })
            .ToListAsync(cancellationToken);
        var values = await _dbContext.TableWidgetValues.AsNoTracking()
            .Where(value => widgets.Any(widget => widget.Id == value.SpecSheetTableWidgetId))
            .OrderBy(value => value.Id)
            .Select(value => new { WidgetId = value.SpecSheetTableWidgetId, value.Value })
            .ToListAsync(cancellationToken);

        return Widgets(
            owners.Select(owner => (owner.Id, owner.OwnerId, owner.TemplateWidgetId)),
            values.Select(value => (value.WidgetId, value.Value)));
    }

    public async Task<Dictionary<int, IReadOnlyList<SnapshotWidgetValue>>> ReadGroupWidgetsAsync(
        IQueryable<DbSpecSheetTableGroup> groups,
        CancellationToken cancellationToken)
    {
        var widgets = _dbContext.SpecSheetTableGroupWidgets.AsNoTracking()
            .Where(widget => groups.Any(group => group.Id == widget.SpecSheetTableGroupId));
        var owners = await widgets
            .OrderBy(widget => widget.Id)
            .Select(widget => new { widget.Id, OwnerId = widget.SpecSheetTableGroupId, TemplateWidgetId = widget.TableGroupTemplateWidgetId })
            .ToListAsync(cancellationToken);
        var values = await _dbContext.TableGroupWidgetValues.AsNoTracking()
            .Where(value => widgets.Any(widget => widget.Id == value.SpecSheetTableGroupWidgetId))
            .OrderBy(value => value.Id)
            .Select(value => new { WidgetId = value.SpecSheetTableGroupWidgetId, value.Value })
            .ToListAsync(cancellationToken);

        return Widgets(
            owners.Select(owner => (owner.Id, owner.OwnerId, owner.TemplateWidgetId)),
            values.Select(value => (value.WidgetId, value.Value)));
    }

    private static Dictionary<int, IReadOnlyList<SnapshotWidgetValue>> Widgets(
        IEnumerable<(int Id, int OwnerId, int TemplateWidgetId)> widgets,
        IEnumerable<(int WidgetId, WidgetValue? Value)> values)
    {
        var valueOf = new Dictionary<int, WidgetValue?>();
        foreach (var (widgetId, value) in values)
        {
            valueOf[widgetId] = value;
        }

        var byOwner = new Dictionary<int, Dictionary<int, WidgetValue?>>();
        foreach (var widget in widgets)
        {
            byOwner.GetOrNew(widget.OwnerId)[widget.TemplateWidgetId] = valueOf.GetValueOrDefault(widget.Id);
        }

        return byOwner.ToDictionary(
            owner => owner.Key,
            owner => (IReadOnlyList<SnapshotWidgetValue>)owner.Value
                .OrderBy(widget => widget.Key)
                .Select(widget => new SnapshotWidgetValue { TemplateWidgetId = widget.Key, Value = widget.Value })
                .ToList());
    }
}
