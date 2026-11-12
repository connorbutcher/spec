using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

public sealed class ColumnBlockDrafts : DraftGateway<SheetColumnBlockRevision>
{
    public ColumnBlockDrafts(
        ISheetColumnBlockRevisionRepository revisions,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
        : base(revisions, users, unitOfWork, currentUser)
    {
    }

    protected override string Subject => "This column block";

    protected override int ItemIdOf(SheetColumnBlockRevision revision)
    {
        return revision.SheetColumnBlockId;
    }
}
