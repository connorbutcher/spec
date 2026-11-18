using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Api.DAL.Query.Mappers;
using PUSpecSheet.Api.DAL.Query.Resolvers.Cells;
using PUSpecSheet.Api.DAL.Query.Resolvers.Tables;
using PUSpecSheet.Query.Models.Table;
using PUSpecSheet.Query.Models.Table.Cell;

namespace PUSpecSheet.Api.DAL.Query.Builders;

/// <summary>
/// Builds the query models of a sheet's tables, groups, rows and cells, following the tables that link to another
/// sheet's table and the cells that link to another sheet's cell.
/// </summary>
internal class SlimTableBuilder
{
    private readonly LinkedTableResolver _linkedTableResolver;
    private readonly LinkedSpecSheetLookupCellResolver _linkedSpecSheetLookupCellResolver;
    private readonly ILogger<SlimTableBuilder> _logger;

    public SlimTableBuilder(LinkedTableResolver linkedTableResolver,
        LinkedSpecSheetLookupCellResolver linkedSpecSheetLookupCellResolver,
        ILogger<SlimTableBuilder> logger)
    {
        ArgumentNullException.ThrowIfNull(linkedTableResolver, nameof(linkedTableResolver));
        ArgumentNullException.ThrowIfNull(linkedSpecSheetLookupCellResolver, nameof(linkedSpecSheetLookupCellResolver));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _linkedTableResolver = linkedTableResolver;
        _linkedSpecSheetLookupCellResolver = linkedSpecSheetLookupCellResolver;
        _logger = logger;
    }

    public async Task<ICollection<SlimPUSpecSheetTable>> BuildTablesAsync(Sheet sheet, SpecSheetType specSheetType, CancellationToken cancellationToken)
    {
        var slimTables = new List<SlimPUSpecSheetTable>();
        foreach (var table in sheet.Tables.OrderBy(x => x.DisplayOrder))
        {
            var tableTemplate = specSheetType.TableTemplates.FirstOrDefault(tt => tt.Id == table.TableTemplateId)
                ?? throw new Exception($"TableTemplate with Id {table.TableTemplateId} not found for SheetTable with Id {table.Id}.");

            var slimTable = await BuildTableAsync(sheet, table, tableTemplate, cancellationToken);
            if (slimTable is not null)
            {
                slimTables.Add(slimTable);
            }
        }

        return slimTables;
    }

    public async Task<SlimPUSpecSheetCell?> BuildCellAsync(Sheet sheet, SheetTableCell cell, CancellationToken cancellationToken)
    {
        return await cell.MapToSlimCell(sheet, _linkedSpecSheetLookupCellResolver, cancellationToken);
    }

    private async Task<SlimPUSpecSheetTable?> BuildTableAsync(Sheet sheet, SheetTable table, TableTemplate tableTemplate, CancellationToken cancellationToken)
    {
        if (table.ConfigurationOverride is not null)
        {
            switch (table.ConfigurationOverride)
            {
                case LinkedTableProperties linkedTableProperties:
                    var tableToMapResult = await _linkedTableResolver.ResolveAsync(sheet, linkedTableProperties, cancellationToken);
                    if (tableToMapResult is null)
                    {
                        _logger.LogWarning("Failed to resolve linked table for SheetTable with Id {SheetTableId} using LinkedTableProperties. Skipping this table.", table.Id);
                        return null;
                    }

                    var tableToMap = tableToMapResult.Result;
                    var tableTemplateForLinkedTable = tableToMapResult.SpecSheetType.TableTemplates.FirstOrDefault(tt => tt.Id == tableToMap.TableTemplateId)
                        ?? throw new Exception($"TableTemplate with Id {tableToMap.TableTemplateId} not found for linked SheetTable with Id {tableToMap.Id}.");

                    return await BuildTableAsync(sheet, tableToMap, tableTemplateForLinkedTable, cancellationToken);
                default:
                    _logger.LogWarning("Unknown ConfigurationOverride type {ConfigurationOverrideType} for SheetTable with Id {SheetTableId}", table.ConfigurationOverride.GetType().Name, table.Id);
                    break;
            }
        }

        var rootGroups = table.Groups.Where(g => g.LinkedGroupReferenceId is null);
        return new SlimPUSpecSheetTable
        {
            Id = table.Id,
            ReferenceId = table.ReferenceId,
            Title = table.Title.ToUpperInvariant(),
            Order = table.DisplayOrder,
            Notes = [.. table.Notes.MapToSlimNotes()],
            Groups = await BuildGroupHierarchyAsync(sheet, rootGroups, table.Groups, tableTemplate, cancellationToken)
        };
    }

    private async Task<ICollection<SlimPUSpecSheetGroup>> BuildGroupHierarchyAsync(Sheet sheet,
        IEnumerable<SheetTableGroup> rootGroups,
        ICollection<SheetTableGroup> allGroups,
        TableTemplate tableTemplate,
        CancellationToken cancellationToken)
    {
        var result = new List<SlimPUSpecSheetGroup>();
        foreach (var group in rootGroups.OrderBy(g => g.DisplayOrder))
        {
            result.Add(await BuildGroupAsync(sheet, group, null, null, allGroups, tableTemplate, cancellationToken));
        }

        return result;
    }

    private async Task<SlimPUSpecSheetGroup> BuildGroupAsync(Sheet sheet,
        SheetTableGroup group,
        int? parentGroupId,
        SheetTableGroup? relatedGroup,
        ICollection<SheetTableGroup> allGroups,
        TableTemplate tableTemplate,
        CancellationToken cancellationToken)
    {
        var groupTemplate = tableTemplate.TableGroupTemplates.FirstOrDefault(gt => gt.Id == group.TableGroupTemplateId)
            ?? throw new Exception($"TableGroupTemplate with Id {group.TableGroupTemplateId} not found for SheetTableGroup with Id {group.Id}.");

        var slimChildGroups = new List<SlimPUSpecSheetGroup>();
        foreach (var childGroup in allGroups.Where(g => g.LinkedGroupReferenceId == group.ReferenceId).OrderBy(g => g.DisplayOrder))
        {
            var linkedGroup = groupTemplate.GroupKey is not null
                ? group
                : null;
            slimChildGroups.Add(await BuildGroupAsync(sheet, childGroup, group.Id, linkedGroup, allGroups, tableTemplate, cancellationToken));
        }

        return new SlimPUSpecSheetGroup
        {
            Id = group.Id,
            ReferenceId = group.ReferenceId,
            ParentGroupId = parentGroupId,
            GroupType = groupTemplate.GroupType,
            Order = group.DisplayOrder,
            Groups = slimChildGroups,
            Rows = await BuildRowsAsync(sheet, group.Rows, relatedGroup?.Rows, cancellationToken)
        };
    }

    private async Task<ICollection<SlimPUSpecSheetRow>> BuildRowsAsync(Sheet sheet, IEnumerable<SheetTableRow> rows, IEnumerable<SheetTableRow>? linkedRows, CancellationToken cancellationToken)
    {
        var result = new List<SlimPUSpecSheetRow>();
        foreach (var row in rows.OrderBy(r => r.DisplayOrder))
        {
            var cells = new List<SlimPUSpecSheetCell>();

            var linkedRow = linkedRows?.FirstOrDefault(lr => lr.DisplayOrder == row.DisplayOrder);
            if (linkedRow is not null)
            {
                cells.AddRange(await BuildCellsAsync(sheet, linkedRow.Cells, cancellationToken));
            }

            cells.AddRange(await BuildCellsAsync(sheet, row.Cells, cancellationToken));

            result.Add(new SlimPUSpecSheetRow
            {
                Id = row.Id,
                ReferenceId = row.ReferenceId,
                Order = row.DisplayOrder,
                Cells = cells
            });
        }

        return result;
    }

    private async Task<ICollection<SlimPUSpecSheetCell>> BuildCellsAsync(Sheet sheet, IEnumerable<SheetTableCell> cells, CancellationToken cancellationToken)
    {
        var result = new List<SlimPUSpecSheetCell>();
        foreach (var cell in cells.OrderBy(c => c.ColumnNumber))
        {
            var slimCell = await BuildCellAsync(sheet, cell, cancellationToken);
            if (slimCell is not null)
            {
                result.Add(slimCell);
            }
        }

        return result;
    }
}
