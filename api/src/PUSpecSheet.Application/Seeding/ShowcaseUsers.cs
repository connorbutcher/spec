using PUSpecSheet.Application.Users;
using PUSpecSheet.Application.Users.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// Switches who the showcase's changes are made as, so versions are published and rows locked by different
/// people: the developer user everyone runs as, and the two extra development users. A user that doesn't
/// exist falls back to the developer.
/// </summary>
public sealed class ShowcaseUsers
{
    public const string Developer = WellKnownUsers.DeveloperUserName;
    public const string EngineerTwo = "engineer2";
    public const string EngineerThree = "engineer3";

    private Dictionary<string, int>? ids;

    private readonly PuSpecSheetDbContext db;
    private readonly BackgroundUser backgroundUser;

    public ShowcaseUsers(PuSpecSheetDbContext db, BackgroundUser backgroundUser)
    {
        this.db = db;
        this.backgroundUser = backgroundUser;
    }

    /// <summary>Makes every change from here on as <paramref name="userName"/>.</summary>
    public async Task ActAsAsync(string userName, CancellationToken cancellationToken)
    {
        if (ids is null)
        {
            string[] userNames = [Developer, EngineerTwo, EngineerThree];
            ids = await UserQueries.GetIdsByUserNameAsync(db, userNames, cancellationToken);
        }

        backgroundUser.UserId = ids.TryGetValue(userName, out var id) ? id : WellKnownUsers.DeveloperId;
    }

    /// <summary>Goes back to the developer user, as start-up work normally runs.</summary>
    public void Reset()
    {
        backgroundUser.UserId = null;
    }
}
