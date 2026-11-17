using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Api.Common.Context;

namespace PUSpecSheet.Api.Common.Authentication;

internal static class AuthenticationHelper
{
    internal const string OID_CLAIM_TYPE = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    internal const string NAME_CLAIM_TYPE = "name";
    internal const string EMAIL_CLAIM_TYPE = ClaimTypes.Email;

    public static async Task<User?> TryGetUserAsync(IUserContext userContext, ClaimsIdentity claimsIdentity, CancellationToken cancellationToken)
    {
        var user = await TryGetUserFromAccountObjectId(userContext, claimsIdentity, cancellationToken);
        if (user == null)
        {
            return user;
        }

        return user;
    }

    public static void CheckAndHandleRobots(this IConfiguration configuration, ClaimsIdentity claimsIdentity)
    {
        var oIdClaim = claimsIdentity.Claims.FirstOrDefault(claim => string.Equals(claim.Type, OID_CLAIM_TYPE, StringComparison.OrdinalIgnoreCase));
        if (oIdClaim == null || string.IsNullOrWhiteSpace(oIdClaim.Value))
        {
            throw new InvalidOperationException("Could not find OID claim");
        }

        var oidClaimValue = Guid.Parse(oIdClaim.Value);
        var robotsSection = configuration.GetRequiredSection("Robots");
        var robotAccounts = robotsSection.GetChildren();

        foreach (var robot in robotAccounts)
        {
            var robotOid = robot.GetValue<Guid>("Oid");
            if (oidClaimValue == robotOid)
            {
                // Seed robot values.
                var robotName = robot.GetValue<string>("Name");
                var robotEmail = robot.GetValue<string>("Email");
                if (string.IsNullOrWhiteSpace(robotName) || string.IsNullOrWhiteSpace(robotEmail))
                {
                    throw new InvalidOperationException("Robot credentials are not properly configured.");
                }

                claimsIdentity.AddClaim(new Claim(EMAIL_CLAIM_TYPE, robotEmail));
                claimsIdentity.AddClaim(new Claim(NAME_CLAIM_TYPE, robotName));
                claimsIdentity.AddClaim(new Claim(OID_CLAIM_TYPE, robotOid.ToString()));

                break;
            }
        }
    }

    private static async Task<User?> TryGetUserFromAccountObjectId(IUserContext userContext, ClaimsIdentity claimsIdentity, CancellationToken cancellationToken)
    {
        var oIdClaim = claimsIdentity.Claims.FirstOrDefault(claim => string.Equals(claim.Type, OID_CLAIM_TYPE, StringComparison.OrdinalIgnoreCase));
        if (oIdClaim != null && !string.IsNullOrWhiteSpace(oIdClaim.Value))
        {
            var objectId = DecodeObjectIdClaim(oIdClaim.Value);
            return await userContext.EnsureUserAsync(objectId, GetName(claimsIdentity), GetEmail(claimsIdentity), cancellationToken);
        }

        return null;
    }

    private static string GetName(ClaimsIdentity claimsIdentity)
    {
        var nameClaim = claimsIdentity.Claims.FirstOrDefault(claim => string.Equals(claim.Type, NAME_CLAIM_TYPE, StringComparison.OrdinalIgnoreCase));
        if (nameClaim == null || string.IsNullOrWhiteSpace(nameClaim.Value))
        {
            throw new InvalidOperationException("Could not find name claim");
        }

        return nameClaim.Value;
    }

    private static string GetEmail(ClaimsIdentity claimsIdentity)
    {
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
}
