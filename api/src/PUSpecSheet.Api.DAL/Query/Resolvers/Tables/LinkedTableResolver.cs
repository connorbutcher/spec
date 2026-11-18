using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.DAL.DataAccess.Types;
using PUSpecSheet.Api.DAL.Query.Resolvers.Models;

namespace PUSpecSheet.Api.DAL.Query.Resolvers.Tables;

internal class LinkedTableResolver : TableResolver<LinkedTableProperties, LinkedResolverResult<SheetTable>>
{
    private readonly ITypeDataAccessService _typeDataAccessService;

    public LinkedTableResolver(ITypeDataAccessService typeDataAccessService,
        LinkedSheetLoader linkedSheetLoader)
        : base(linkedSheetLoader)
    {
        _typeDataAccessService = typeDataAccessService;
    }

    public override async Task<LinkedResolverResult<SheetTable>?> ResolveAsync(Sheet sheet, LinkedTableProperties data, CancellationToken cancellationToken)
    {
        if (data.TableReferenceId is null || data.SpecSheetTypeId is null || data.SpecSheetPhaseId is null || sheet.Version is null)
        {
            return null;
        }

        var latestSheet = await _linkedSheetLoader.LoadAsync(
            data.SpecSheetTypeId.Value,
            data.SpecSheetPhaseId.Value,
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

        var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(latestSheet.SpecSheetTypeId, cancellationToken)
            ?? throw new Exception($"SpecSheetType with Id {latestSheet.SpecSheetTypeId} not found for Sheet with Id {latestSheet.Id}.");

        return new LinkedResolverResult<SheetTable>
        {
            Result = referencedTable,
            SpecSheetType = specSheetType,
        };
    }
}
