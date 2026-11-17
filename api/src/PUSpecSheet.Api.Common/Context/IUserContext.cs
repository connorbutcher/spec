using PUSpecSheet.Abstractions.Domain.Access;

namespace PUSpecSheet.Api.Common.Context;

/// <summary>
/// Defines the user context.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Ensure the user has access, otherwise create the user.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the request.</param>
    /// <returns>The user.</returns>
    Task<User> EnsureUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensure the user has access, otherwise create the user.
    /// </summary>
    /// <param name="objectId">The users object id.</param>
    /// <param name="displayName">The users name.</param>
    /// <param name="emailAddress">The users email.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the request.</param>
    /// <returns>The user.</returns>
    Task<User> EnsureUserAsync(Guid objectId, string displayName, string emailAddress, CancellationToken cancellationToken = default);
}
