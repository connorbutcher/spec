using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Tests.Published;

/// <summary>A selection's key names the answer, so it has to be the same for the same question and different for a different one.</summary>
public sealed class PublishedSheetSelectionTests
{
    private const string First = "a41e0000-0000-0000-0000-000000000001";
    private const string Second = "b7d00000-0000-0000-0000-000000000002";

    private static readonly ResolvedSheetVersion Version = new(1, Guid.Parse(First), 7, DateTime.UnixEpoch);

    [Fact]
    public void TheSameIdentifiersInAnotherOrder_GiveTheSameKey()
    {
        var one = PublishedSheetSelection.Parse(null, null, null, $"{First},{Second}", PublishedSheetShape.Flat, null);
        var other = PublishedSheetSelection.Parse(null, null, null, $" {Second} , {First},", PublishedSheetShape.Flat, null);

        Assert.Equal(one.Key, other.Key);
        Assert.Equal(Version.KeyFor(one), Version.KeyFor(other));
    }

    [Fact]
    public void ADifferentQuestion_GivesADifferentKey()
    {
        var keys = new[]
        {
            PublishedSheetSelection.Parse(null, null, null, null, PublishedSheetShape.Tree, null).Key,
            PublishedSheetSelection.Parse(null, null, null, null, PublishedSheetShape.Flat, null).Key,
            PublishedSheetSelection.Parse(null, null, null, null, PublishedSheetShape.Tree, "labels").Key,
            PublishedSheetSelection.Parse(null, null, null, First, PublishedSheetShape.Tree, null).Key,
            PublishedSheetSelection.Parse(null, null, First, null, PublishedSheetShape.Tree, null).Key,
            PublishedSheetSelection.Parse(null, null, null, Second, PublishedSheetShape.Tree, null).Key,
        };

        Assert.Equal(keys.Length, keys.Distinct().Count());
    }

    [Fact]
    public void ARequestBody_GivesTheSameKeyAsTheSameQueryString()
    {
        var body = PublishedSheetSelection.From(new PublishedSheetQueryRequest(null, null, null, [Guid.Parse(Second), Guid.Parse(First)], PublishedSheetShape.Flat));
        var query = PublishedSheetSelection.Parse(null, null, null, $"{First},{Second}", PublishedSheetShape.Flat, null);

        Assert.Equal(query.Key, body.Key);
    }

    [Fact]
    public void TheKey_NamesTheSheetAndVersion()
    {
        var everything = PublishedSheetSelection.Parse(null, null, null, null, PublishedSheetShape.Tree, null);

        Assert.True(everything.IsEverything);
        Assert.Equal("a41e0000000000000000000000000001-v7-t-all", Version.KeyFor(everything));
    }

    [Theory]
    [InlineData("not-an-id", null)]
    [InlineData(null, "captions")]
    public void WhatCannotBeUnderstood_IsRejected(string? cells, string? include)
    {
        Assert.Throws<InvalidRequestException>(
            () => PublishedSheetSelection.Parse(null, null, null, cells, PublishedSheetShape.Tree, include));
    }
}
