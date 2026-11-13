using PUSpecSheet.Migrator.Building;

namespace PUSpecSheet.Migrator.Tests.Building;

/// <summary>Each letter is one item's signature; an underscore is an item with an empty one.</summary>
public sealed class SiblingMatcherTests
{
    [Fact]
    public void UnchangedItems_KeepTheirPartners()
    {
        int[] expected = [0, 1, 2];
        Assert.Equal(expected, Match("abc", "abc"));
    }

    [Fact]
    public void AnItemAddedInTheMiddle_IsNewAndLeavesTheOthersAlone()
    {
        int[] expected = [0, -1, 1, 2];
        Assert.Equal(expected, Match("abc", "axbc"));
    }

    [Fact]
    public void AnItemChangedInPlace_IsPairedWithItsOldSelf()
    {
        int[] expected = [0, 1, 2];
        Assert.Equal(expected, Match("abc", "aBc"));
    }

    [Fact]
    public void AnItemRemoved_LeavesItsOldSelfUnpaired()
    {
        int[] expected = [0, 2];
        Assert.Equal(expected, Match("abc", "ac"));
    }

    [Fact]
    public void ItemsThatLookAlike_ArePairedInOrder()
    {
        int[] expected = [0, 1];
        Assert.Equal(expected, Match("___", "__"));
    }

    [Fact]
    public void EverythingNew_HasNoPartners()
    {
        int[] expected = [-1, -1];
        Assert.Equal(expected, Match(string.Empty, "ab"));
    }

    private static int[] Match(string previous, string current)
    {
        return SiblingMatcher.Match(Signatures(previous), Signatures(current));
    }

    private static List<string> Signatures(string letters)
    {
        return letters.Select(letter => letter == '_' ? string.Empty : new string(letter, 1)).ToList();
    }
}
