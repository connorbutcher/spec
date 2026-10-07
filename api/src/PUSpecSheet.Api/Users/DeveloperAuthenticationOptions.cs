using Microsoft.AspNetCore.Authentication;

namespace PUSpecSheet.Api.Users;

/// <summary>The "DeveloperSignIn" configuration section.</summary>
public sealed class DeveloperAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SectionName = "DeveloperSignIn";

    /// <summary>
    /// Lets a request say which user it runs as (see <see cref="DeveloperAuthenticationHandler"/>), so
    /// multi-user editing can be tried from two browser tabs. Only ever on in Development.
    /// </summary>
    public bool AllowUserSwitching { get; set; }
}
