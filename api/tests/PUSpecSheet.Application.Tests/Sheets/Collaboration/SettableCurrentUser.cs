using PUSpecSheet.Application.Users;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

/// <summary>The user a test is acting as, changed between steps to play more than one person.</summary>
internal sealed class SettableCurrentUser : ICurrentUser
{
    public int UserId { get; set; }
}
