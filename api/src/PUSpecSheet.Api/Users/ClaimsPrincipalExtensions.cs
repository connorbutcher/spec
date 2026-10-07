using System.Globalization;
using System.Security.Claims;

namespace PUSpecSheet.Api.Users;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The id of the user a signed-in principal stands for, from its name identifier claim. Null when
    /// nobody is signed in. Whatever sign-in is used, this claim is how the API knows who is calling.
    /// </summary>
    public static int? FindUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var userId) ? userId : null;
    }
}
