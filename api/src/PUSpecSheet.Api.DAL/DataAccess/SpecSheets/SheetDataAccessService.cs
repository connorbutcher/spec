using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Sheets;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

internal class SheetDataAccessService : ISheetDataAccessService
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly SheetReader _sheetReader;
    private readonly IUserContext _userContext;

    public SheetDataAccessService(PUSpecSheetDbContext dbContext, SheetReader sheetReader, IUserContext userContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(sheetReader, nameof(sheetReader));
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));

        _dbContext = dbContext;
        _sheetReader = sheetReader;
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
