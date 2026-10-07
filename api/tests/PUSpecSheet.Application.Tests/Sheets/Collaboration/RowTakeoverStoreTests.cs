using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

public sealed class RowTakeoverStoreTests
{
    private static readonly DateTime Asked = new(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ARow_HasOneRequestWaitingAtATime()
    {
        var store = new RowTakeoverStore();

        Assert.True(store.TryAdd(Takeover(rowId: 10, requester: 2)));
        Assert.False(store.TryAdd(Takeover(rowId: 10, requester: 3)));
        Assert.Equal(2, store.ForRow(10)!.RequesterUserId);
    }

    [Fact]
    public void ARequest_CanOnlyBeSettledOnce()
    {
        var store = new RowTakeoverStore();
        var takeover = Takeover(rowId: 10, requester: 2);
        store.TryAdd(takeover);

        Assert.True(store.TryRemove(takeover));
        Assert.False(store.TryRemove(takeover));
        Assert.Null(store.Find(takeover.Id));
    }

    [Fact]
    public void ARequest_IsDue_OnceItsTimeToAnswerHasRunOut()
    {
        var store = new RowTakeoverStore();
        store.TryAdd(Takeover(rowId: 10, requester: 2));

        Assert.Empty(store.Due(Asked.AddSeconds(59)));
        Assert.Single(store.Due(Asked.AddSeconds(60)));
    }

    [Fact]
    public void SomeoneJoining_GetsTheRequestsTheyMadeOrHaveToAnswer()
    {
        var store = new RowTakeoverStore();
        store.TryAdd(Takeover(rowId: 10, requester: 2, holder: 1));
        store.TryAdd(Takeover(rowId: 11, requester: 3, holder: 2));
        store.TryAdd(Takeover(rowId: 12, requester: 3, holder: 1));
        store.TryAdd(Takeover(rowId: 13, requester: 2, holder: 1, sheetId: 9));

        Assert.Equal([10, 11], store.Involving(sheetId: 1, userId: 2).Select(takeover => takeover.RowId).Order());
    }

    private static RowTakeoverDto Takeover(int rowId, int requester, int holder = 1, int sheetId = 1)
    {
        return new RowTakeoverDto(
            Guid.NewGuid(),
            sheetId,
            rowId,
            requester,
            "Requester",
            holder,
            "Holder",
            Asked,
            Asked.AddSeconds(60),
            RowTakeoverStatus.Pending);
    }
}
