namespace PUSpecSheet.Database.Models.Users;

public class DbUserRole
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public virtual DbUser User { get; set; } = null!;

    public virtual DbRole Role { get; set; } = null!;
}
