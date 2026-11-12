using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>Draft handling for sheet tables. A new draft also carries over the table's title.</summary>
public sealed class TableDrafts : DraftGateway<SheetTableRevision>
{
    public TableDrafts(ISheetTableRevisionRepository revisions, IUserRepository users, IUnitOfWork unitOfWork, ICurrentUser currentUser)
        : base(revisions, users, unitOfWork, currentUser)
    {
    }

    protected override string Subject => "This table";

    protected override int ItemIdOf(SheetTableRevision revision)
    {
        return revision.SheetTableId;
    }

    /// <summary>A table is unchanged when its place, existence and title are.</summary>
    protected override Task<HashSet<SheetTableRevision>> UnchangedAsync(
        IReadOnlyList<(SheetTableRevision Draft, SheetTableRevision Current)> pairs,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(pairs
            .Where(pair => SheetRevisionComparer.SameStructure(pair.Draft, pair.Current)
                && SheetRevisionComparer.SameText(pair.Draft.Title, pair.Current.Title))
            .Select(pair => pair.Draft)
            .ToHashSet());
    }
}
