using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Data.Tests.Users;

/// <summary>The roles and permissions every database starts with.</summary>
public sealed class PermissionSeedTests : IDisposable
{
    private readonly PuSpecSheetDbContext db = new(
        new DbContextOptionsBuilder<PuSpecSheetDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options);

    /// <summary>The model as migrations see it, which is the one that carries seed data.</summary>
    private IModel Model => db.GetService<IDesignTimeModel>().Model;

    public void Dispose()
    {
        db.Dispose();
    }

    [Fact]
    public void EveryPermissionKey_HasARowInThePermissionsTable()
    {
        var seeded = SeedValues<Permission>(nameof(Permission.Key)).Cast<string>().Order(StringComparer.Ordinal);

        Assert.Equal(PermissionKeys.All.Order(StringComparer.Ordinal), seeded);
    }

    [Fact]
    public void TheDeveloperUser_IsAnAdministrator()
    {
        var link = Assert.Single(Model.FindEntityType(typeof(UserRole))!.GetSeedData());

        Assert.Equal(WellKnownUsers.DeveloperId, link[nameof(UserRole.UserId)]);
        Assert.Equal(WellKnownRoles.AdministratorId, link[nameof(UserRole.RoleId)]);
    }

    [Fact]
    public void TheAdministratorRole_IsSeededWithNoPermissionsListed()
    {
        Assert.Equal([WellKnownRoles.AdministratorId], SeedValues<Role>(nameof(Role.Id)).Cast<int>());
        Assert.Empty(Model.FindEntityType(typeof(RolePermission))!.GetSeedData());
    }

    private IEnumerable<object?> SeedValues<T>(string property)
    {
        return Model.FindEntityType(typeof(T))!.GetSeedData().Select(row => row[property]);
    }
}
