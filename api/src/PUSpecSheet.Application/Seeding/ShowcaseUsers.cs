using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// Switches who the showcase's changes are made as, so versions are published and rows locked by different
/// people: the developer user everyone runs as, and the two extra development users. A user that doesn't
/// exist falls back to the developer.
/// </summary>
public sealed class ShowcaseUsers(PuSpecSheetDbContext db, BackgroundUser backgroundUser)
{
    public const string Developer = WellKnownUsers.DeveloperUserName;
    public const string EngineerTwo = "engineer2";
    public const string EngineerThree = "engineer3";

    private Dictionary<string, int>? ids;

    /// <summary>Makes every change from here on as <paramref name="userName"/>.</summary>
    public async Task ActAsAsync(string userName, CancellationToken cancellationToken)
    {
        if (ids is null)
        {
            string[] userNames = [Developer, EngineerTwo, EngineerThree];
            ids = await db.Users
                .Where(user => userNames.Contains(user.UserName))
                .ToDictionaryAsync(user => user.UserName, user => user.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        }

        backgroundUser.UserId = ids.TryGetValue(userName, out var id) ? id : WellKnownUsers.DeveloperId;
    }

    /// <summary>Goes back to the developer user, as start-up work normally runs.</summary>
    public void Reset()
    {
        backgroundUser.UserId = null;
    }
}
