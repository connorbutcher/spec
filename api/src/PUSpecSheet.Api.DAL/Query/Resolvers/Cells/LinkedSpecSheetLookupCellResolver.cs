using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Abstractions.Domain.Legacy.Values;
using PUSpecSheet.Api.DAL.Caching;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets;
using PUSpecSheet.Api.DAL.Query.Mappers;
using PUSpecSheet.Api.DAL.Query.Resolvers.Tables;
using PUSpecSheet.Query.Models.Table.Cell;
using PUSpecSheet.Query.Models.Values;

namespace PUSpecSheet.Api.DAL.Query.Resolvers.Cells;

internal class LinkedSpecSheetLookupCellResolver : CellResolver<LinkedSpecSheetValue, SlimPUSpecSheetCell>
{
    private readonly ISpecSheetDataAccessService _specSheetDataAccessService;
    private readonly LinkedTableResolver _linkedTableResolver;

    public LinkedSpecSheetLookupCellResolver(ISpecSheetDataAccessService specSheetDataAccessService,
        ISpecSheetCache loadedSpecSheetsService,
        LinkedTableResolver linkedTableResolver)
        : base(loadedSpecSheetsService)
    {
        _specSheetDataAccessService = specSheetDataAccessService;
        _linkedTableResolver = linkedTableResolver;
    }

    public override async Task<SlimPUSpecSheetCell?> ResolveAsync(SpecSheet specSheet, LinkedSpecSheetValue data, CancellationToken cancellationToken)
    {
        if (data.TableReferenceId is null || data.LinkedReferenceId is null || data.LinkedSpecSheetPhaseId is null || data.LinkedSpecSheetTypeId is null)
        {
            return null;
        }

        var latestSpecSheetSummary = await _specSheetDataAccessService.GetLatestPublishedSpecSheetSummaryByTypeAndPhaseAsync(
            data.LinkedSpecSheetTypeId.Value,
            data.LinkedSpecSheetPhaseId.Value,
            specSheet.PublishedDateTime!.Value,
            cancellationToken);
        if (latestSpecSheetSummary is null)
        {
            return null;
        }

        var latestSpecSheet = await _loadedSpecSheetsService.GetAsync(latestSpecSheetSummary.Id, cancellationToken);
        if (latestSpecSheet is null)
        {
            latestSpecSheet = await _specSheetDataAccessService.GetSpecSheetByIdAsync(latestSpecSheetSummary.Id, cancellationToken);
            if (latestSpecSheet is null)
            {
                return null;
            }

            await _loadedSpecSheetsService.SetAsync(latestSpecSheet, cancellationToken);
        }

        var referencedTable = latestSpecSheet.Tables.FirstOrDefault(t => t.ReferenceId == data.TableReferenceId.Value);
        if (referencedTable is null)
        {
            return null;
        }

        if (referencedTable.ConfigurationOverride is not null && referencedTable.ConfigurationOverride is LinkedTableProperties linkedTableProperties)
        {
            var table = await _linkedTableResolver.ResolveAsync(latestSpecSheet, linkedTableProperties, cancellationToken);
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

        var mappedCell = await referencedCell.MapToSlimCell(specSheet, this, cancellationToken);
        var linkedValue = new SlimLinkedValue
        {
            Id = referencedCell.CellValue!.Id,
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
