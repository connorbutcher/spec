using System.Text.RegularExpressions;
using PUSpecSheet.Abstractions.Users;

namespace PUSpecSheet.DataAccess.Tests.Database.Users;

/// <summary>
/// The roles and permissions every database starts with, added by the database project's post-deployment
/// script (database/Script.PostDeployment.sql).
/// </summary>
public sealed partial class PermissionSeedTests
{
    private static readonly string _script = File.ReadAllText(FindPostDeploymentScript()).ReplaceLineEndings("\n");

    [Fact]
    public void EveryPermissionKey_HasARowInThePermissionsTable()
    {
        var block = Block("Permissions");
        var seeded = PermissionRow().Matches(block).Select(match => match.Groups["key"].Value).Order(StringComparer.Ordinal);

        Assert.Equal(PermissionKeys.All.Order(StringComparer.Ordinal), seeded);
    }

    [Fact]
    public void TheDeveloperUser_IsAnAdministrator()
    {
        Assert.Contains(
            $"INSERT INTO [auth].[Users] ([Id], [UserName], [DisplayName], [IsActive], [CreatedAtUtc])\nSELECT {WellKnownUsers.DeveloperId}, N'{WellKnownUsers.DeveloperUserName}'",
            _script,
            StringComparison.Ordinal);
        Assert.Contains(
            $"INSERT INTO [auth].[UserRoles] ([UserId], [RoleId])\nSELECT {WellKnownUsers.DeveloperId}, {WellKnownRoles.AdministratorId}",
            _script,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheAdministratorRole_IsSeededWithNoPermissionsListed()
    {
        Assert.Contains(
            $"INSERT INTO [auth].[Roles] ([Id], [Name], [Description])\nSELECT {WellKnownRoles.AdministratorId}, N'{WellKnownRoles.AdministratorName}'",
            _script,
            StringComparison.Ordinal);
        Assert.DoesNotContain("[auth].[RolePermissions]", _script, StringComparison.Ordinal);
    }

    /// <summary>The script's statements for one table, from its IDENTITY_INSERT ON to its OFF.</summary>
    private static string Block(string table)
    {
        var start = _script.IndexOf($"SET IDENTITY_INSERT [auth].[{table}] ON;", StringComparison.Ordinal);
        var end = _script.IndexOf($"SET IDENTITY_INSERT [auth].[{table}] OFF;", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, $"The post-deployment script has no {table} block.");
        return _script[start..end];
    }

    private static string FindPostDeploymentScript()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var script = Path.Combine(directory.FullName, "database", "Script.PostDeployment.sql");
            if (File.Exists(script))
            {
                return script;
            }
        }

        throw new FileNotFoundException("database/Script.PostDeployment.sql wasn't found above the test output folder.");
    }

    [GeneratedRegex(@"^\s*\(\d+, N'(?<key>[^']+)', N'", RegexOptions.Multiline)]
    private static partial Regex PermissionRow();
}
