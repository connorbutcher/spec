using PUSpecSheet.Api.Common.Context;

namespace PUSpecSheet.Api.Common.Claims;

/// <summary>
/// The claims identity context accessor.
/// </summary>
public class ClaimsIdentityContextAccessor : IIdentityContextAccessor
{
    private readonly IClaimsAccessor _claimsAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClaimsIdentityContextAccessor"/> class.
    /// </summary>
    public ClaimsIdentityContextAccessor(IClaimsAccessor claimsAccessor)
    {
        _claimsAccessor = claimsAccessor;
    }

    ///<inheritdoc/>
    public Guid ObjectGuid => _claimsAccessor.GetObjectId();

    ///<inheritdoc/>
    public string Name => _claimsAccessor.GetName();

    ///<inheritdoc/>
    public string Email => _claimsAccessor.GetEmail();
}
