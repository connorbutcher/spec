namespace PUSpecSheet.CheckInOut.Api.Models;

/// <summary>
/// Data class representing a user connected to the SignalR hub.
/// </summary>
public record ConnectedUser(string ConnectionId, string Username, string Uid, string Colour);
