using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.DAL.Context;

internal class UserContext : IUserContext
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly ILogger<UserContext> _logger;

    public UserContext(
        PUSpecSheetDbContext dbContext,
        IIdentityContextAccessor identityContextAccessor,
        ILogger<UserContext> logger)
    {
        _dbContext = dbContext;
        _identityContextAccessor = identityContextAccessor;
        _logger = logger;
    }

    public async Task<User> EnsureUserAsync(CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)
            .Include(x => x.TablePermissions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.ObjectId == _identityContextAccessor.ObjectGuid, cancellationToken)
            ?? await CreateUserAsync(
                _identityContextAccessor.ObjectGuid,
                _identityContextAccessor.Name,
                _identityContextAccessor.Email,
                _dbContext,
                cancellationToken);

        return user.MapToDomain();
    }

    public async Task<User> EnsureUserAsync(Guid objectId, string displayName, string emailAddress, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.ObjectId == objectId, cancellationToken)
            ?? await CreateUserAsync(objectId, displayName, emailAddress, _dbContext, cancellationToken);

        return user.MapToDomain();
    }

    private async Task<DbUser> CreateUserAsync(Guid objectId, string displayName, string emailAddress, PUSpecSheetDbContext buildCardDbContext, CancellationToken cancellationToken)
    {
        var dbUser = await buildCardDbContext.Users.AddAsync(new DbUser
        {
            Id = 0,
            ObjectId = objectId,
            DisplayName = displayName,
            EmailAddress = emailAddress
        }, cancellationToken);

        await buildCardDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User created: {User}", dbUser.Entity.ObjectId);

        return dbUser.Entity;
    }
}
