using PUSpecSheet.Application.Users;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Users;

/// <summary>
/// The user the request was signed in as, read from its claims. Until sign-in is added that is the seeded
/// developer user (see <see cref="DeveloperAuthenticationHandler"/>), who is also who work outside a
/// request runs as, such as seeding at start-up, unless that work names someone else through
/// <see cref="BackgroundUser"/>. Real authentication only has to issue the same claim.
/// </summary>
public sealed class DeveloperCurrentUser(IHttpContextAccessor httpContext, BackgroundUser backgroundUser) : ICurrentUser
{
    public int UserId => httpContext.HttpContext?.User.FindUserId() ?? backgroundUser.UserId ?? WellKnownUsers.DeveloperId;
}
