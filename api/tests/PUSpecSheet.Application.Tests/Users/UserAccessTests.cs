using PUSpecSheet.Application.Users;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Application.Tests.Users;

/// <summary>What a user may do, worked out from their roles.</summary>
public sealed class UserAccessTests
{
    [Fact]
    public void AnAdministrator_HasEveryPermissionWithoutAnyBeingGranted()
    {
        var user = UserWith(Role(WellKnownRoles.AdministratorId, WellKnownRoles.AdministratorName));

        var access = UserAccess.From(user);

        Assert.True(access.IsAdministrator);
        Assert.Equal(PermissionKeys.All, access.Permissions);
        Assert.All(PermissionKeys.All, permission => Assert.True(access.Has(permission)));
        Assert.True(access.Has("something.addedLater"));
    }

    [Fact]
    public void AnotherRole_GivesOnlyThePermissionsGrantedToIt()
    {
        var user = UserWith(Role(2, "Phase editor", PermissionKeys.PhasesManage));

        var access = UserAccess.From(user);

        Assert.False(access.IsAdministrator);
        Assert.Equal([PermissionKeys.PhasesManage], access.Permissions);
        Assert.True(access.Has(PermissionKeys.PhasesManage));
        Assert.False(access.Has(PermissionKeys.SheetTypesManage));
    }

    [Fact]
    public void SeveralRoles_CombineTheirPermissionsOnce()
    {
        var user = UserWith(
            Role(2, "Phase editor", PermissionKeys.PhasesManage),
            Role(3, "Setup", PermissionKeys.SheetTypesManage, PermissionKeys.PhasesManage));

        var access = UserAccess.From(user);

        Assert.Equal([PermissionKeys.PhasesManage, PermissionKeys.SheetTypesManage], access.Permissions);
        Assert.Equal(["Phase editor", "Setup"], access.Roles);
    }

    [Fact]
    public void ARoleNamedAdministrator_IsNotTheAdministratorRole()
    {
        var user = UserWith(Role(9, WellKnownRoles.AdministratorName));

        var access = UserAccess.From(user);

        Assert.False(access.IsAdministrator);
        Assert.False(access.Has(PermissionKeys.PhasesManage));
    }

    [Fact]
    public void AUserWithNoRoles_MayDoNothing()
    {
        var access = UserAccess.From(UserWith());

        Assert.Empty(access.Permissions);
        Assert.False(access.Has(PermissionKeys.PhasesManage));
    }

    [Fact]
    public void AnInactiveUser_LosesWhatTheirRolesAllowed()
    {
        var user = UserWith(Role(WellKnownRoles.AdministratorId, WellKnownRoles.AdministratorName));
        user.IsActive = false;

        var access = UserAccess.From(user);

        Assert.False(access.IsAdministrator);
        Assert.Empty(access.Roles);
        Assert.False(access.Has(PermissionKeys.PhasesManage));
    }

    [Fact]
    public void AUserThatDoesNotExist_MayDoNothing()
    {
        Assert.False(UserAccess.None(42).Has(PermissionKeys.PhasesManage));
    }

    private static User UserWith(params Role[] roles)
    {
        var user = new User { Id = 5, UserName = "tester", DisplayName = "Tester" };
        foreach (var role in roles)
        {
            user.Roles.Add(new UserRole { User = user, UserId = user.Id, Role = role, RoleId = role.Id });
        }

        return user;
    }

    private static Role Role(int id, string name, params string[] permissions)
    {
        var role = new Role { Id = id, Name = name };
        foreach (var key in permissions)
        {
            role.Permissions.Add(new RolePermission { Role = role, RoleId = id, Permission = new Permission { Key = key } });
        }

        return role;
    }
}
