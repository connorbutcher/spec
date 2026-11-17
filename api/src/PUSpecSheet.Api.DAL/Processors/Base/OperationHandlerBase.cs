using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Base;

internal abstract class OperationHandlerBase
{
    protected OperationHandlerBase(PUSpecSheetDbContext dbContext, User user)
    {
        DbContext = dbContext;
        User = user;
    }

    protected PUSpecSheetDbContext DbContext { get; }

    protected User User { get; }

    protected static int ResolveId(int id, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(id) ? tempIdMap[id] : id;
    }

    protected static bool IsTempId(int id)
    {
        return id < 0;
    }
}
