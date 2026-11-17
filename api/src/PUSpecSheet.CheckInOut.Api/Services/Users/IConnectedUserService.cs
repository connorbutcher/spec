using PUSpecSheet.CheckInOut.Api.Models;

namespace PUSpecSheet.CheckInOut.Api.Services.Users;

/// <summary>
/// Defines a service maintaining a list of the connected users.
/// </summary>
public interface IConnectedUserService
{
    /// <summary>
    /// Gets all the client contexts.
    /// </summary>
    /// <returns>The client contexts.</returns>
    Task<IReadOnlyList<ConnectedUser>> GetAllConnectedUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Try and add a client connection.
    /// </summary>
    /// <param name="connectionId">The connection id of the client to add.</param>
    /// <param name="username">The username of the client.</param>
    /// <param name="uid">The users UID.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> TryAddUserAsync(string connectionId, string username, string uid);

    /// <summary>
    /// Try and get a client context.
    /// </summary>
    /// <param name="connectionId">The connection id of the client to get.</param>
    /// <param name="clientContext">The client context.</param>
    /// <returns>The connected user context or null if no user is found.</returns>
    Task<ConnectedUser?> GetConnectedUserAsync(string connectionId);

    /// <summary>
    /// Try and remove a client.
    /// </summary>
    /// <param name="connectionId">The connection id of the client to remove.</param>
    Task RemoveConnectedUserAsync(string connectionId);
}
