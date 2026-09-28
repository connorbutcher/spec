using PUSpecSheet.Application.Users;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Users;

/// <summary>
/// Runs every request as the seeded developer user until sign-in is added; real authentication
/// replaces this single registration with a claims-backed implementation.
/// </summary>
public sealed class DeveloperCurrentUser : ICurrentUser
{
    public int UserId => WellKnownUsers.DeveloperId;
}
