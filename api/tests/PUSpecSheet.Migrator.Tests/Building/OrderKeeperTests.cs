using PUSpecSheet.Migrator.Building;

namespace PUSpecSheet.Migrator.Tests.Building;

public sealed class OrderKeeperTests
{
    [Fact]
    public void NewItems_AreSpacedOut()
    {
        int?[] existing = [null, null, null];
        int[] expected = [1024, 2048, 3072];
        Assert.Equal(expected, OrderKeeper.Assign(existing));
    }

    [Fact]
    public void AnItemInsertedBetweenTwo_GoesBetweenThemAndTheyKeepTheirOrders()
    {
        int?[] existing = [1024, null, 2048];
        int[] expected = [1024, 1536, 2048];
        Assert.Equal(expected, OrderKeeper.Assign(existing));
    }

    [Fact]
    public void ItemsAddedAtEitherEnd_GoBeyondTheEnds()
    {
        int?[] existing = [null, 1024, null];
        int[] expected = [0, 1024, 2048];
        Assert.Equal(expected, OrderKeeper.Assign(existing));
    }

    [Fact]
    public void AnItemOutOfOrder_IsTheOnlyOneGivenANewOrder()
    {
        int?[] existing = [1024, 3072, 2048, 4096];
        var orders = OrderKeeper.Assign(existing);

        Assert.Equal(1024, orders[0]);
        Assert.Equal(4096, orders[3]);
        Assert.Equal(1, new[] { orders[1] != 3072, orders[2] != 2048 }.Count(changed => changed));
        Assert.True(orders[0] < orders[1] && orders[1] < orders[2] && orders[2] < orders[3]);
    }

    [Fact]
    public void WhenThereIsNoRoom_EverythingIsNumberedAfresh()
    {
        int?[] existing = [1, null, 2];
        int[] expected = [1024, 2048, 3072];
        Assert.Equal(expected, OrderKeeper.Assign(existing));
    }
}
