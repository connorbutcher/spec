using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PUSpecSheet.Api.Common.Claims;

public class ClaimsAccessor : IClaimsAccessor
{
    private const string OID_CLAIM_TYPE = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    private const string NAME_CLAIM_TYPE = "name";
    private const string EMAIL_CLAIM_TYPE = ClaimTypes.Email;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetObjectId()
    {
        var claimsIdentity = GetClaimsIdentity();

        var oIdClaim = claimsIdentity.Claims.FirstOrDefault(claim => string.Equals(claim.Type, OID_CLAIM_TYPE, StringComparison.OrdinalIgnoreCase));
        if (oIdClaim == null || string.IsNullOrWhiteSpace(oIdClaim.Value))
        {
            throw new InvalidOperationException("Could not find OID claim");
        }

        return DecodeObjectIdClaim(oIdClaim.Value);
    }

    public string GetName()
    {
        var claimsIdentity = GetClaimsIdentity();

        var nameClaim = claimsIdentity.Claims.FirstOrDefault(claim => string.Equals(claim.Type, NAME_CLAIM_TYPE, StringComparison.OrdinalIgnoreCase));
        if (nameClaim == null || string.IsNullOrWhiteSpace(nameClaim.Value))
        {
            throw new InvalidOperationException("Could not find name claim");
        }

        return nameClaim.Value;
    }

    public string? TryGetName()
    {
        if (_httpContextAccessor?.HttpContext?.User.Identity is not ClaimsIdentity claimsIdentity)
        {
            return null;
        }

        var nameClaim = claimsIdentity.Claims.FirstOrDefault(claim => string.Equals(claim.Type, NAME_CLAIM_TYPE, StringComparison.OrdinalIgnoreCase));
        return nameClaim?.Value;
    }

    public string GetEmail()
    {
        var claimsIdentity = GetClaimsIdentity();

        var emailClaim = claimsIdentity.Claims.FirstOrDefault(claim => string.Equals(claim.Type, EMAIL_CLAIM_TYPE, StringComparison.OrdinalIgnoreCase));
        if (emailClaim == null || string.IsNullOrWhiteSpace(emailClaim.Value))
        {
            throw new InvalidOperationException("Could not find email claim");
        }

        return emailClaim.Value;
    }

    private static Guid DecodeObjectIdClaim(string oidClaim)
    {
        var trimmedOidClaim = oidClaim.Trim();

        var buffer = new Span<byte>(new byte[trimmedOidClaim.Length]);
        if (Convert.TryFromBase64String(trimmedOidClaim, buffer, out var bytesParsed))
        {
            return new Guid(buffer[..bytesParsed]);
        }

        return Guid.Parse(trimmedOidClaim);
    }

    private ClaimsIdentity GetClaimsIdentity()
    {
        if (_httpContextAccessor?.HttpContext?.User.Identity is not ClaimsIdentity claimsIdentity)
        {
            throw new InvalidOperationException("Could not find ClaimsIdentity");
        }

        if (!claimsIdentity.IsAuthenticated)
        {
            throw new InvalidOperationException("ClaimsIdentity is not authenticated");
        }

        return claimsIdentity;
    }
}
