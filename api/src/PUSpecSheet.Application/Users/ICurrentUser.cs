namespace PUSpecSheet.Application.Users;

/// <summary>The user the current request runs as: who holds locks and publishes.</summary>
public interface ICurrentUser
{
    int UserId { get; }
}
