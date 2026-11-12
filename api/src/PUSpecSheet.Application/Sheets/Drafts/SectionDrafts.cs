using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

public sealed class SectionDrafts(
    ISheetSectionRevisionRepository revisions,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : DraftGateway<SheetSectionRevision>(revisions, users, unitOfWork, currentUser)
{
    protected override string Subject => "This section";

    protected override int ItemIdOf(SheetSectionRevision revision)
    {
        return revision.SheetSectionId;
    }
}
