using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>The drafts in a publish scope (the publisher's own or everyone's) that aren't already waiting for approval.</summary>
public sealed record ScopeDraftSelection(PublishScope Scope, int PublisherUserId) : IDraftSelection
{
    public IQueryable<TRevision> Apply<TRevision>(IQueryable<TRevision> revisions)
        where TRevision : class, ISheetRevision
    {
        return revisions.DraftsIn(Scope, PublisherUserId);
    }
}
