using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Requests.Access;
using PUSpecSheet.Abstractions.Requests.Access.Tables;
using PUSpecSheet.Api.DAL.Caching;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Sheets;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.DAL.DataAccess.Admin;

internal class AdminDataAccessService : IAdminDataAccessService
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly ISpecSheetCache _specSheetCache;
    private readonly SheetReader _sheetReader;
    private readonly ILogger<IAdminDataAccessService> _logger;

    public AdminDataAccessService(PUSpecSheetDbContext dbContext,
        ISpecSheetCache specSheetCache,
        SheetReader sheetReader,
        ILogger<IAdminDataAccessService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _dbContext = dbContext;
        _specSheetCache = specSheetCache;
        _sheetReader = sheetReader;
        _logger = logger;
    }

    ///<inheritdoc />
    public async Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)
            .Include(x => x.TablePermissions)
            .MapToDomain()
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> CreateUserAsync(CreateUserRequest createUserRequest, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(createUserRequest, nameof(createUserRequest));

        var user = new DbUser
        {
            Id = 0,
            ObjectId = createUserRequest.ObjectId,
            DisplayName = createUserRequest.DisplayName,
            EmailAddress = createUserRequest.Email
        };

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Created user with Object ID {ObjectId}.", createUserRequest.ObjectId);

        return user.Id;
    }

    ///<inheritdoc />
    public async Task<IReadOnlyList<Permission>> GetPermissions()
    {
        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .MapToDomain()
            .ToListAsync()
            ?? throw new InvalidOperationException("Unable to get the permissions");

        _logger.LogInformation("Retrieved {PermissionCount} permissions", permissions.Count);
        return permissions;
    }

    ///<inheritdoc />
    public async Task<IReadOnlyList<SpecSheetTablePermission>> GetTablePermissionsByReferenceIdAsync(Guid referenceId, CancellationToken cancellationToken)
    {
        var tablePermission = await _dbContext.SpecSheetTablePermissions
            .Where(tp => tp.TableReferenceId == referenceId)
            .AsNoTracking()
            .MapToDomain()
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} table permissions with Reference ID {ReferenceId}.", tablePermission.Count, referenceId);
        return tablePermission;
    }

    ///<inheritdoc />
    public async Task UpdateUserPermissionsAsync(UpdateUserPermissionsRequest updateUserPermissionsRequest, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(updateUserPermissionsRequest, nameof(updateUserPermissionsRequest));

        var user = await _dbContext.Users
            .Where(u => u.Id == updateUserPermissionsRequest.UserId)
            .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception($"User with ID {updateUserPermissionsRequest.UserId} not found.");

        foreach (var PermissionIdToRemove in updateUserPermissionsRequest.PermissionIdsToRemove)
        {
            var permission = user.Permissions.FirstOrDefault(x => x.PermissionId == PermissionIdToRemove);
            if (permission != null)
            {
                _dbContext.UserPermissions.Remove(permission);
            }
        }

        foreach (var PermissionIdToAdd in updateUserPermissionsRequest.PermissionIdsToAdd)
        {
            var permission = user.Permissions.FirstOrDefault(x => x.PermissionId == PermissionIdToAdd);
            if (permission == null)
            {
                _dbContext.UserPermissions.Add(new DbUserPermission
                {
                    Id = 0,
                    UserId = user.Id,
                    PermissionId = PermissionIdToAdd
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateTablePermissionsAsync(ManageTablePermissionsRequest manageTablePermissionsRequest, CancellationToken cancellationToken)
    {
        var existingPermissions = await _dbContext.SpecSheetTablePermissions
            .Where(tp => tp.TableReferenceId == manageTablePermissionsRequest.TableReferenceId)
            .ToListAsync(cancellationToken);

        foreach (var idToDelete in manageTablePermissionsRequest.DeleteTablePermissionRequests)
        {
            var permission = existingPermissions.FirstOrDefault(tp => tp.Id == idToDelete);
            if (permission != null)
            {
                _dbContext.SpecSheetTablePermissions.Remove(permission);
            }
        }

        foreach (var permissionToAdd in manageTablePermissionsRequest.CreateTablePermissionRequests)
        {
            var exists = existingPermissions.Any(tp =>
                tp.UserId == permissionToAdd.UserId &&
                tp.TableReferenceId == manageTablePermissionsRequest.TableReferenceId &&
                tp.IsAdmin == permissionToAdd.IsAdmin);
            if (exists)
            {
                continue;
            }

            var newPermission = new DbSpecSheetTablePermission
            {
                Id = 0,
                UserId = permissionToAdd.UserId,
                TableReferenceId = manageTablePermissionsRequest.TableReferenceId,
                IsAdmin = permissionToAdd.IsAdmin
            };
            _dbContext.SpecSheetTablePermissions.Add(newPermission);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated table permissions for Table Reference ID {ReferenceId}.", manageTablePermissionsRequest.TableReferenceId);
    }

    public async Task DeleteTablePermissionsByReferenceIdAsync(Guid referenceId, CancellationToken cancellationToken)
    {
        var tablePermissions = await _dbContext.SpecSheetTablePermissions
            .Where(tp => tp.TableReferenceId == referenceId)
            .ToListAsync(cancellationToken)
            ?? throw new Exception($"Table permissions with Reference ID {referenceId} not found.");

        _dbContext.SpecSheetTablePermissions.RemoveRange(tablePermissions);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Deleted {Count} table permissions with Reference ID {ReferenceId}.", tablePermissions.Count, referenceId);
    }

    public async Task RefreshCacheForSpecSheetTypeAsync(int specSheetTypeId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refreshing cache for Spec Sheet Type ID {SpecSheetTypeId}.", specSheetTypeId);

        await foreach (var cacheKey in _specSheetCache.GetAllKeysAsync(cancellationToken))
        {
            var keyString = cacheKey.Split(':').LastOrDefault();

            if (string.IsNullOrWhiteSpace(keyString) || !int.TryParse(keyString, out var key))
            {
                _logger.LogWarning("Skipping cache key {CacheKey} as it does not contain a valid integer ID.", cacheKey);
                continue;
            }

            var version = await FindVersionOfTypeAsync(key, specSheetTypeId, cancellationToken);
            if (version is not null)
            {
                await _specSheetCache.RemoveAsync(key, cancellationToken);
                _logger.LogInformation("Removed cache entry for Spec Sheet ID {SpecSheetId} with Spec Sheet Type ID {SpecSheetTypeId}.", key, specSheetTypeId);

                var sheet = await _sheetReader.ReadPublishedAsync(version.Value.SheetId, version.Value.VersionNumber, cancellationToken);
                if (sheet is not null)
                {
                    await _specSheetCache.SetAsync(sheet, cancellationToken);
                }
                _logger.LogInformation("Refreshed cache entry for Spec Sheet ID {SpecSheetId} with Spec Sheet Type ID {SpecSheetTypeId}.", key, specSheetTypeId);
            }
        }
    }

    public async Task ResetCacheForSpecSheetTypeAsync(int specSheetTypeId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resetting cache for Spec Sheet Type ID {SpecSheetTypeId}.", specSheetTypeId);

        await foreach (var cacheKey in _specSheetCache.GetAllKeysAsync(cancellationToken))
        {
            var keyString = cacheKey.Split(':').LastOrDefault();

            if (string.IsNullOrWhiteSpace(keyString) || !int.TryParse(keyString, out var key))
            {
                _logger.LogWarning("Skipping cache key {CacheKey} as it does not contain a valid integer ID.", cacheKey);
                continue;
            }

            var version = await FindVersionOfTypeAsync(key, specSheetTypeId, cancellationToken);
            if (version is not null)
            {
                await _specSheetCache.RemoveAsync(key, cancellationToken);
                _logger.LogInformation("Removed cache entry for Spec Sheet ID {SpecSheetId} with Spec Sheet Type ID {SpecSheetTypeId}.", key, specSheetTypeId);
            }
        }
    }

    // Where the version that the cache entry for the spec sheet is of: null when it isn't a version of a sheet of the type.
    private async Task<(int SheetId, int VersionNumber)?> FindVersionOfTypeAsync(int specSheetId, int specSheetTypeId, CancellationToken cancellationToken)
    {
        var version = await (from v in _dbContext.SheetVersions.AsNoTracking()
                             join s in _dbContext.Sheets.AsNoTracking() on v.SheetId equals s.Id
                             where v.SourceSpecSheetId == specSheetId && s.SpecSheetTypeId == specSheetTypeId
                             select new { v.SheetId, v.VersionNumber })
            .FirstOrDefaultAsync(cancellationToken);

        return version is null ? null : (version.SheetId, version.VersionNumber);
    }
}
