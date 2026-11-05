namespace PUSpecSheet.Application.Users;

/// <summary>
/// Who work outside a request runs as, such as seeding at start-up, for the life of one scope. Null, the
/// default, leaves it to <see cref="ICurrentUser"/>'s own rule; the sample data sets it to show changes,
/// publishes and locks by more than one person. A request never sets it.
/// </summary>
public sealed class BackgroundUser
{
    public int? UserId { get; set; }
}
