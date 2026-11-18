namespace PUSpecSheet.Api.Common.Claims;

/// <summary>
/// Provides the claims of the <see cref="DefaultUser"/> for every request, standing in for <see cref="ClaimsAccessor"/>
/// while auth is disabled for local running.
/// </summary>
public class DefaultUserClaimsAccessor : IClaimsAccessor
{
    ///<inheritdoc/>
    public Guid GetObjectId()
    {
        return DefaultUser.ObjectId;
    }

    ///<inheritdoc/>
    public string GetName()
    {
        return DefaultUser.Name;
    }

    ///<inheritdoc/>
    public string? TryGetName()
    {
        return DefaultUser.Name;
    }

    ///<inheritdoc/>
    public string GetEmail()
    {
        return DefaultUser.Email;
    }
}
