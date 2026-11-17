using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Domain.SpecSheets;
using PUSpecSheet.Api.DAL.Caching;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets;
using PUSpecSheet.Api.DAL.DataAccess.Types;
using PUSpecSheet.Api.DAL.Query.Resolvers.Models;

namespace PUSpecSheet.Api.DAL.Query.Resolvers.Tables;

internal class LinkedTableResolver : TableResolver<LinkedTableProperties, LinkedResolverResult<SpecSheetTable>>
{
    private readonly ISpecSheetDataAccessService _specSheetDataAccessService;
    private readonly ITypeDataAccessService _typeDataAccessService;

    public LinkedTableResolver(ISpecSheetDataAccessService specSheetDataAccessService,
        ITypeDataAccessService typeDataAccessService,
        ISpecSheetCache loadedSpecSheetsService)
        : base(loadedSpecSheetsService)
    {
        _specSheetDataAccessService = specSheetDataAccessService;
        _typeDataAccessService = typeDataAccessService;
    }

    public override async Task<LinkedResolverResult<SpecSheetTable>?> ResolveAsync(SpecSheet specSheet, LinkedTableProperties data, CancellationToken cancellationToken)
    {
        if (data.TableReferenceId is null || data.SpecSheetTypeId is null || data.SpecSheetPhaseId is null)
        {
            return null;
        }

        var latestSpecSheetSummary = await _specSheetDataAccessService.GetLatestPublishedSpecSheetSummaryByTypeAndPhaseAsync(
            data.SpecSheetTypeId.Value,
            data.SpecSheetPhaseId.Value,
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

        var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(specSheet.SpecSheetTypeId, cancellationToken)
            ?? throw new Exception($"SpecSheetType with Id {specSheet.SpecSheetTypeId} not found for SpecSheet with Id {specSheet.Id}.");

        return new LinkedResolverResult<SpecSheetTable>
        {
            Result = referencedTable,
            SpecSheetType = specSheetType,
        };
    }
}
