namespace PUSpecSheet.Api.Common.Claims;

/// <summary>
/// The users a request can act as while auth is disabled for local running, so that checkouts and approvals can be tried
/// with more than one user. A request picks one with the <see cref="HeaderName"/> header (1, 2 or 3); without it, it is
/// the <see cref="DefaultUser"/>.
/// </summary>
/// <remarks>
/// database/seed-local-users.sql gives the second and third the default user's permissions and makes all three
/// approvers of every phase, so keep the two in step.
/// </remarks>
public static class LocalUsers
{
    /// <summary>
    /// The request header naming which local user the request acts as.
    /// </summary>
    public const string HeaderName = "X-Local-User";

    public static readonly IReadOnlyList<LocalUser> All =
    [
        new LocalUser { ObjectId = DefaultUser.ObjectId, Name = DefaultUser.Name, Email = DefaultUser.Email },
        new LocalUser { ObjectId = new Guid("00000000-0000-0000-0000-000000000002"), Name = "Local Engineer", Email = "local.engineer@localhost" },
        new LocalUser { ObjectId = new Guid("00000000-0000-0000-0000-000000000003"), Name = "Local Approver", Email = "local.approver@localhost" }
    ];

    /// <summary>
    /// The local user with the number given in the header (counting from 1), or the default user when the value is
    /// missing or not one of them.
    /// </summary>
    public static LocalUser Find(string? headerValue)
    {
        if (int.TryParse(headerValue, out var number) && number >= 1 && number <= All.Count)
        {
            return All[number - 1];
        }

        return All[0];
    }
}
