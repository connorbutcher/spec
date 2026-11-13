namespace PUSpecSheet.Database.Models.Users;

public class DbUserRole
{
    public int UserId { get; set; }

    public DbUser User { get; set; } = null!;

    public int RoleId { get; set; }

    public DbRole Role { get; set; } = null!;
}
