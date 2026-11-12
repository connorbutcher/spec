using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

public sealed class RowDrafts(
    ISheetRowRevisionRepository revisions,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IRowValueRepository values) : DraftGateway<SheetRowRevision>(revisions, users, unitOfWork, currentUser)
{
    protected override string Subject => "This row";

    protected override int ItemIdOf(SheetRowRevision revision)
    {
        return revision.SheetRowId;
    }

    /// <summary>A row is unchanged when its place and existence are, and every cell holds what is published.</summary>
    protected override async Task<HashSet<SheetRowRevision>> UnchangedAsync(
        IReadOnlyList<(SheetRowRevision Draft, SheetRowRevision Current)> pairs,
        CancellationToken cancellationToken)
    {
        var sameStructure = pairs
            .Where(pair => SheetRevisionComparer.SameStructure(pair.Draft, pair.Current))
            .ToList();
        if (sameStructure.Count == 0)
        {
            return [];
        }

        var revisionIds = sameStructure.SelectMany(pair => new[] { pair.Draft.Id, pair.Current.Id }).ToList();
        var held = await values.LoadAsync(revisionIds, cancellationToken);
        return sameStructure
            .Where(pair => CellValuesComparer.Same(held.GetValueOrDefault(pair.Draft.Id), held.GetValueOrDefault(pair.Current.Id)))
            .Select(pair => pair.Draft)
            .ToHashSet();
    }
}
