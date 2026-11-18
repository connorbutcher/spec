using Microsoft.AspNetCore.Http;

namespace PUSpecSheet.Api.Common.Claims;

/// <summary>
/// Provides the claims of one of the <see cref="LocalUsers"/> for every request, standing in for
/// <see cref="ClaimsAccessor"/> while auth is disabled for local running. The request's
/// <see cref="LocalUsers.HeaderName"/> header picks which; without it every request is the <see cref="DefaultUser"/>.
/// </summary>
public class DefaultUserClaimsAccessor : IClaimsAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultUserClaimsAccessor(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));

        _httpContextAccessor = httpContextAccessor;
    }

    ///<inheritdoc/>
    public Guid GetObjectId()
    {
        return CurrentUser().ObjectId;
    }

    ///<inheritdoc/>
    public string GetName()
    {
        return CurrentUser().Name;
    }

    ///<inheritdoc/>
    public string? TryGetName()
    {
        return CurrentUser().Name;
    }

    ///<inheritdoc/>
    public string GetEmail()
    {
        return CurrentUser().Email;
    }

    private LocalUser CurrentUser()
    {
        var headerValue = _httpContextAccessor.HttpContext?.Request.Headers[LocalUsers.HeaderName].FirstOrDefault();
        return LocalUsers.Find(headerValue);
    }
}
