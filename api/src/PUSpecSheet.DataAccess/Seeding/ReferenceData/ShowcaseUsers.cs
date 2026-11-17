using PUSpecSheet.Abstractions.Users;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.DataAccess.Users.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Seeding.ReferenceData;

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

    private Dictionary<string, int>? _ids;

    private readonly PuSpecSheetDbContext _db;
    private readonly BackgroundUser _backgroundUser;

    public ShowcaseUsers(PuSpecSheetDbContext db, BackgroundUser backgroundUser)
    {
        _db = db;
        _backgroundUser = backgroundUser;
    }

    /// <summary>Makes every change from here on as <paramref name="userName"/>.</summary>
    public async Task ActAsAsync(string userName, CancellationToken cancellationToken)
    {
        if (_ids is null)
        {
            string[] userNames = [Developer, EngineerTwo, EngineerThree];
            _ids = await UserQueries.GetIdsByUserNameAsync(_db, userNames, cancellationToken);
        }

        _backgroundUser.UserId = _ids.TryGetValue(userName, out var id) ? id : WellKnownUsers.DeveloperId;
    }

    /// <summary>Goes back to the developer user, as start-up work normally runs.</summary>
    public void Reset()
    {
        _backgroundUser.UserId = null;
    }
}
