using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Tests.Sheets;

/// <summary>What each person sees of an item that has a published revision, a draft, or both.</summary>
public sealed class RevisionResolverTests
{
    private const int Me = 1;
    private const int SomeoneElse = 2;

    [Fact]
    public void MyOwnDraft_IsWhatIAmShown_AndIsTheLock()
    {
        var published = Published(rowId: 5);
        var draft = Draft(rowId: 5, author: Me);

        var resolution = Resolve(published, draft)[5];

        Assert.Same(draft, resolution.Shown);
        Assert.Same(draft, resolution.Draft);
    }

    [Fact]
    public void SomeoneElsesDraft_LocksTheItem_ButIStillSeeWhatIsPublished()
    {
        var published = Published(rowId: 5);
        var draft = Draft(rowId: 5, author: SomeoneElse);

        var resolution = Resolve(published, draft)[5];

        Assert.Same(published, resolution.Shown);
        Assert.Same(draft, resolution.Draft);
    }

    [Fact]
    public void AnItemOnlySomeoneElseHasDrafted_IsNotVisibleToMe()
    {
        var resolution = Resolve(Draft(rowId: 5, author: SomeoneElse))[5];

        Assert.Null(resolution.Shown);
        Assert.False(RevisionResolver.IsVisible(resolution));
    }

    [Fact]
    public void ARemovalIAmDrafting_HidesTheItemFromMeOnly()
    {
        var published = Published(rowId: 5);
        var removal = Draft(rowId: 5, author: Me);
        removal.IsDeleted = true;

        Assert.False(RevisionResolver.IsVisible(Resolve(published, removal)[5]));
        Assert.True(RevisionResolver.IsVisible(RevisionResolver.Resolve([published, removal], revision => revision.SheetRowId, SomeoneElse)[5]));
    }

    [Fact]
    public void EachItemIsResolvedOnItsOwn()
    {
        var resolved = Resolve(Published(rowId: 5), Published(rowId: 6), Draft(rowId: 6, author: Me));

        Assert.Equal([5, 6], resolved.Keys.Order());
        Assert.Null(resolved[5].Draft);
        Assert.NotNull(resolved[6].Draft);
    }

    [Fact]
    public void NothingAtAll_IsNotVisible()
    {
        Assert.False(RevisionResolver.IsVisible<SheetRowRevision>(null));
    }

    private static Dictionary<int, RevisionResolution<SheetRowRevision>> Resolve(params SheetRowRevision[] revisions)
    {
        return RevisionResolver.Resolve(revisions, revision => revision.SheetRowId, Me);
    }

    private static SheetRowRevision Published(int rowId)
    {
        return new SheetRowRevision { SheetRowId = rowId, Status = RevisionStatus.Published, AuthorUserId = SomeoneElse };
    }

    private static SheetRowRevision Draft(int rowId, int author)
    {
        return new SheetRowRevision { SheetRowId = rowId, Status = RevisionStatus.Draft, AuthorUserId = author };
    }
}
