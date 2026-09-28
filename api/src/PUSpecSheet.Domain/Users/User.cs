namespace PUSpecSheet.Domain.Users;

/// <summary>A person who can edit and publish sheets.</summary>
public class User
{
    public int Id { get; set; }

    /// <summary>The unique sign-in name, e.g. a Windows account name.</summary>
    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Email { get; set; }

    /// <summary>Inactive users keep their history but can't sign in or hold locks.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
}
