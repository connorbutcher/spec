using PUSpecSheet.Application.Sheets.Removal;

namespace PUSpecSheet.Application.Tests.Sheets.Removal;

/// <summary>Alex (user 2) changes something inside a section or table that may be being removed.</summary>
public sealed class PendingRemovalsTests
{
    private const int Sam = 1;
    private const int Alex = 2;

    [Fact]
    public void NothingBeingRemoved_LetsTheChangeThrough()
    {
        Assert.Null(PendingRemovals.Blocking([], Alex));
    }

    [Fact]
    public void MyOwnOpenRemoval_DoesNotStopMe()
    {
        Assert.Null(PendingRemovals.Blocking([new PendingRemoval(RemovedContainer.Section, Alex, false)], Alex));
    }

    [Fact]
    public void SomeoneElsesRemoval_StopsTheChange_ByName()
    {
        var blocking = PendingRemovals.Blocking([new PendingRemoval(RemovedContainer.Section, Sam, false)], Alex);

        Assert.NotNull(blocking);
        Assert.Equal(
            "Sam is removing the section this is in, so it can't be changed unless they put it back.",
            PendingRemovals.RefusalFor(blocking, "Sam"));
    }

    [Fact]
    public void ARemovalWaitingForApproval_StopsEveryone_IncludingItsAuthor()
    {
        var blocking = PendingRemovals.Blocking([new PendingRemoval(RemovedContainer.Section, Alex, true)], Alex);

        Assert.NotNull(blocking);
        Assert.Equal(
            "Removing the section this is in is waiting for approval, so it can't be changed until that is decided.",
            PendingRemovals.RefusalFor(blocking, "Alex"));
    }

    [Fact]
    public void TheTableBeingRemoved_IsNamedBeforeASectionInIt()
    {
        var blocking = PendingRemovals.Blocking(
            [new PendingRemoval(RemovedContainer.Section, Sam, false), new PendingRemoval(RemovedContainer.Table, Sam, false)],
            Alex);

        Assert.NotNull(blocking);
        Assert.Equal(
            "Sam is removing the table this is in, so it can't be changed unless they put it back.",
            PendingRemovals.RefusalFor(blocking, "Sam"));
    }
}
