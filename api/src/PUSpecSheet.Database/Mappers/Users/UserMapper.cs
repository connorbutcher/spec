using System.Linq.Expressions;
using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Database.Models.Users;

namespace PUSpecSheet.Database.Mappers.Users;

/// <summary>Users as DTOs.</summary>
public static class UserMapper
{
    /// <summary>A user as someone who can be asked to approve, as an expression so a query selects only these two columns.</summary>
    public static readonly Expression<Func<DbUser, ApproverDto>> ApproverProjection = user => new ApproverDto
    {
        UserId = user.Id,
        UserName = user.DisplayName,
    };
}
