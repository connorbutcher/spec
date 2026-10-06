using PUSpecSheet.Application.Common;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Tests.Common;

/// <summary>Template items keep a gapless 1-based order among their siblings.</summary>
public sealed class DisplayOrderingTests
{
    [Theory]
    [InlineData(1, "C,A,B")]
    [InlineData(2, "A,C,B")]
    [InlineData(3, "A,B,C")]
    [InlineData(0, "C,A,B")]
    [InlineData(99, "A,B,C")]
    public void Move_PutsTheItemAtThePosition_ClampedToTheGroup(int position, string expected)
    {
        var rows = Rows(("A", 1), ("B", 2), ("C", 3));
        var moved = rows.Single(row => row.Name == "C");

        DisplayOrdering.Move(rows, moved, position, row => row.DisplayOrder, (row, order) => row.DisplayOrder = order);

        Assert.Equal(expected, string.Join(',', rows.OrderBy(row => row.DisplayOrder).Select(row => row.Name)));
        Assert.Equal([1, 2, 3], rows.Select(row => row.DisplayOrder).Order());
    }

    [Fact]
    public void Renumber_ClosesGapsAndKeepsTheOrder()
    {
        var rows = Rows(("A", 4), ("B", 9), ("C", 2));

        DisplayOrdering.Renumber(rows, row => row.DisplayOrder, (row, order) => row.DisplayOrder = order);

        Assert.Equal("C,A,B", string.Join(',', rows.OrderBy(row => row.DisplayOrder).Select(row => row.Name)));
        Assert.Equal([1, 2, 3], rows.Select(row => row.DisplayOrder).Order());
    }

    private static List<TemplateSection> Rows(params (string Name, int Order)[] items)
    {
        return items.Select(item => new TemplateSection { Name = item.Name, DisplayOrder = item.Order }).ToList();
    }
}
