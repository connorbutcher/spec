using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>The drafts in a publish scope (the publisher's own or everyone's) that aren't already waiting for approval.</summary>
public sealed record ScopeDraftSelection : IDraftSelection
{
    public ScopeDraftSelection(PublishScope scope, int publisherUserId)
    {
        this.Scope = scope;
        this.PublisherUserId = publisherUserId;
    }

    public PublishScope Scope { get; init; }

    public int PublisherUserId { get; init; }

    public IQueryable<TRevision> Apply<TRevision>(IQueryable<TRevision> revisions)
        where TRevision : class, ISheetRevision
    {
        return revisions.DraftsIn(Scope, PublisherUserId);
    }
}
