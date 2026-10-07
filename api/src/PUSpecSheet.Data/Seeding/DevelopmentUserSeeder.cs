using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Data.Seeding;

/// <summary>
/// Adds two more users to a development database, so multi-user editing can be tried before sign-in
/// exists: open the app in a second tab as one of them (see the README). Does nothing once they exist.
/// </summary>
public static class DevelopmentUserSeeder
{
    private static readonly (string UserName, string DisplayName)[] Users =
    [
        ("engineer2", "Engineer Two"),
        ("engineer3", "Engineer Three"),
    ];

    public static async Task SeedAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken = default)
    {
        var userNames = Users.Select(user => user.UserName).ToList();
        var existing = await db.Users
            .Where(user => userNames.Contains(user.UserName))
            .Select(user => user.UserName)
            .ToListAsync(cancellationToken);

        foreach (var (userName, displayName) in Users.Where(user => !existing.Contains(user.UserName, StringComparer.OrdinalIgnoreCase)))
        {
            var user = new User { UserName = userName, DisplayName = displayName, CreatedAtUtc = DateTime.UtcNow };
            user.Roles.Add(new UserRole { RoleId = WellKnownRoles.AdministratorId });
            db.Users.Add(user);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
