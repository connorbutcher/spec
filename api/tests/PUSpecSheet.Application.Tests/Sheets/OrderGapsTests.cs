using PUSpecSheet.Application.Sheets;

namespace PUSpecSheet.Application.Tests.Sheets;

/// <summary>Moving something and moving it back must leave exactly what was published, so nothing needs publishing.</summary>
public sealed class OrderGapsTests
{
    // Three siblings published at 1024, 2048 and 3072. The item being moved is the middle one, 2048.
    private static readonly int[] OthersWithoutMiddle = [1024, 3072];

    [Fact]
    public void MovingAnItemToWhereItAlreadyIs_KeepsItsPublishedOrder()
    {
        Assert.Equal(2048, OrderGaps.PlaceAt(OthersWithoutMiddle, position: 2, publishedOrder: 2048));
    }

    [Fact]
    public void MovingAnItemAway_GivesItANewOrder()
    {
        var order = OrderGaps.PlaceAt(OthersWithoutMiddle, position: 3, publishedOrder: 2048);

        Assert.True(order > 3072);
        Assert.NotEqual(2048, order);
    }

    [Fact]
    public void MovingAnItemAwayAndBack_ReturnsToThePublishedOrder()
    {
        // Down to last: a new order past the others. Back to second: its published order again.
        var away = OrderGaps.PlaceAt(OthersWithoutMiddle, position: 3, publishedOrder: 2048);
        var back = OrderGaps.PlaceAt(OthersWithoutMiddle, position: 2, publishedOrder: 2048);

        Assert.NotEqual(away, back);
        Assert.Equal(2048, back);
    }

    [Fact]
    public void WithNoPublishedOrder_ItPlacesBetweenNeighboursAsBefore()
    {
        Assert.Equal(2048, OrderGaps.PlaceAt(OthersWithoutMiddle, position: 2, publishedOrder: null));
    }

    [Fact]
    public void WhenAnotherSiblingNowSitsOnThePublishedOrder_ItIsNotReused()
    {
        // Another item has taken 2048, so the published order isn't free to go back to.
        var order = OrderGaps.PlaceAt([1024, 2048, 3072], position: 2, publishedOrder: 2048);

        Assert.NotEqual(2048, order);
    }
}
