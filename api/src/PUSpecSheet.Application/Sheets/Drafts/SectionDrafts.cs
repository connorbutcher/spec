using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

public sealed class SectionDrafts : DraftGateway<SheetSectionRevision>
{
    public SectionDrafts(ISheetSectionRevisionRepository revisions, IUserRepository users, IUnitOfWork unitOfWork, ICurrentUser currentUser)
        : base(revisions, users, unitOfWork, currentUser)
    {
    }

    protected override string Subject => "This section";

    protected override int ItemIdOf(SheetSectionRevision revision)
    {
        return revision.SheetSectionId;
    }
}
