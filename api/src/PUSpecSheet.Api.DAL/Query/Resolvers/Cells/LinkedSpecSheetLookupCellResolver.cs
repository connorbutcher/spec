using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.DAL.Query.Mappers;
using PUSpecSheet.Api.DAL.Query.Resolvers.Tables;
using PUSpecSheet.Query.Models.Table.Cell;
using PUSpecSheet.Query.Models.Values;

namespace PUSpecSheet.Api.DAL.Query.Resolvers.Cells;

internal class LinkedSpecSheetLookupCellResolver : CellResolver<SheetLinkedSheetValue, SlimPUSpecSheetCell>
{
    private readonly LinkedTableResolver _linkedTableResolver;

    public LinkedSpecSheetLookupCellResolver(LinkedSheetLoader linkedSheetLoader,
        LinkedTableResolver linkedTableResolver)
        : base(linkedSheetLoader)
    {
        _linkedTableResolver = linkedTableResolver;
    }

    public override async Task<SlimPUSpecSheetCell?> ResolveAsync(Sheet sheet, SheetLinkedSheetValue data, CancellationToken cancellationToken)
    {
        if (data.TableReferenceId is null || data.LinkedReferenceId is null || data.LinkedSpecSheetPhaseId is null || data.LinkedSpecSheetTypeId is null || sheet.Version is null)
        {
            return null;
        }

        var latestSheet = await _linkedSheetLoader.LoadAsync(
            data.LinkedSpecSheetTypeId.Value,
            data.LinkedSpecSheetPhaseId.Value,
            sheet.Version.PublishedDateTime,
            cancellationToken);
        if (latestSheet is null)
        {
            return null;
        }

        var referencedTable = latestSheet.Tables.FirstOrDefault(t => t.ReferenceId == data.TableReferenceId.Value);
        if (referencedTable is null)
        {
            return null;
        }

        if (referencedTable.ConfigurationOverride is LinkedTableProperties linkedTableProperties)
        {
            var table = await _linkedTableResolver.ResolveAsync(latestSheet, linkedTableProperties, cancellationToken);
            if (table is null)
            {
                return null;
            }

            referencedTable = table.Result;
        }

        // Flatten all the cells in the table and find the one with the matching reference id
        var referencedCell = referencedTable.Groups
            .SelectMany(g => g.Rows)
            .SelectMany(r => r.Cells)
            .FirstOrDefault(c => c.ReferenceId == data.LinkedReferenceId.Value);
        if (referencedCell is null)
        {
            return null;
        }

        var mappedCell = await referencedCell.MapToSlimCell(sheet, this, cancellationToken);
        var linkedValue = new SlimLinkedValue
        {
            Id = referencedCell.Value.SheetTableCellId,
            LinkedReferenceId = referencedCell.ReferenceId,
            SpecSheetTypeId = data.LinkedSpecSheetTypeId.Value,
            SpecSheetPhaseId = data.LinkedSpecSheetPhaseId.Value,
            TableName = referencedTable.Title,
            TableReferenceId = data.TableReferenceId.Value,
            Value = mappedCell!.Value
        };

        return mappedCell with
        {
            Value = linkedValue
        };
    }
}
