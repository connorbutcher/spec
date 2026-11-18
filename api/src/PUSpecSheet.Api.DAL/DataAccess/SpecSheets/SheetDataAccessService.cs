using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Sheets;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

internal class SheetDataAccessService : ISheetDataAccessService
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly SheetReader _sheetReader;
    private readonly SheetEditSwitch _editSwitch;
    private readonly IUserContext _userContext;

    public SheetDataAccessService(PUSpecSheetDbContext dbContext, SheetReader sheetReader, SheetEditSwitch editSwitch, IUserContext userContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(sheetReader, nameof(sheetReader));
        ArgumentNullException.ThrowIfNull(editSwitch, nameof(editSwitch));
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));

        _dbContext = dbContext;
        _sheetReader = sheetReader;
        _editSwitch = editSwitch;
        _userContext = userContext;
    }

    public async Task<Sheet?> GetPublishedSheetAsync(int sheetId, int? versionNumber, CancellationToken cancellationToken)
    {
        var sheet = await _sheetReader.ReadPublishedAsync(sheetId, versionNumber, cancellationToken);
        return sheet is null ? null : await TrimTablesAsync(sheet, cancellationToken);
    }

    public async Task<Sheet?> GetDraftSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        var sheet = await _sheetReader.ReadDraftAsync(sheetId, cancellationToken);
        return sheet is null ? null : await TrimTablesAsync(sheet, cancellationToken);
    }

    public async Task<IReadOnlyList<SheetVersion>> GetSheetVersionsAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _sheetReader.ReadVersionsAsync(sheetId, cancellationToken);
    }

    public async Task<Sheet?> GetLatestPublishedSheetByPhaseAndTypeAsync(int phaseId, int sheetTypeId, DateTime? publishedBy, CancellationToken cancellationToken)
    {
        var sheetId = await _sheetReader.FindSheetIdAsync(phaseId, sheetTypeId, cancellationToken);
        if (sheetId is null)
        {
            return null;
        }

        var sheet = await _sheetReader.ReadPublishedByAsync(sheetId.Value, publishedBy, cancellationToken);
        return sheet is null ? null : await TrimTablesAsync(sheet, cancellationToken);
    }

    public async Task<IReadOnlyList<SheetSummary>> GetSheetSummariesByPhaseAsync(int phaseId, bool includeChildren, CancellationToken cancellationToken)
    {
        var phaseIds = new List<int> { phaseId };
        if (includeChildren)
        {
            phaseIds.AddRange(await _dbContext.SpecSheetPhases
                .AsNoTracking()
                .Where(phase => phase.ParentSpecSheetPhaseId == phaseId)
                .Select(phase => phase.Id)
                .ToListAsync(cancellationToken));
        }

        return await _sheetReader.ReadSummariesAsync(sheet => phaseIds.Contains(sheet.SpecSheetPhaseId), cancellationToken);
    }

    public async Task<IReadOnlyList<SheetSummary>> GetSheetSummariesByTypeAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await _sheetReader.ReadSummariesAsync(sheet => sheet.SpecSheetTypeId == sheetTypeId, cancellationToken);
    }

    public async Task<SheetSummary?> GetSheetSummaryByPhaseAndTypeAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        var summaries = await _sheetReader.ReadSummariesAsync(sheet => sheet.SpecSheetPhaseId == phaseId && sheet.SpecSheetTypeId == sheetTypeId, cancellationToken);
        return summaries.FirstOrDefault();
    }

    public async Task<SheetLocation?> FindSheetAsync(int specSheetId, CancellationToken cancellationToken)
    {
        return await _sheetReader.FindLocationAsync(specSheetId, _editSwitch, cancellationToken);
    }

    // A table that has permissions is only shown to the users it has them for, as a copy's is.
    private async Task<Sheet> TrimTablesAsync(Sheet sheet, CancellationToken cancellationToken)
    {
        var currentUser = await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Current user not found in the context.");

        var tableReferenceIds = sheet.Tables.Select(table => table.ReferenceId).ToList();
        var permissions = await _dbContext.SpecSheetTablePermissions
            .AsNoTracking()
            .Where(permission => tableReferenceIds.Contains(permission.TableReferenceId))
            .ToListAsync(cancellationToken);

        var tables = sheet.Tables
            .Where(table => !permissions.Any(permission => permission.TableReferenceId == table.ReferenceId)
                || permissions.Any(permission => permission.TableReferenceId == table.ReferenceId && permission.UserId == currentUser.Id))
            .ToList();

        return sheet with { Tables = tables };
    }
}
