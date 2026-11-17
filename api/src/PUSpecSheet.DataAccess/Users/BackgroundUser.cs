namespace PUSpecSheet.DataAccess.Users;

// Who work outside a request (start-up seeding) runs as. Null leaves it to ICurrentUser; a request never sets it.
public sealed class BackgroundUser
{
    public int? UserId { get; set; }
}
