using PUSpecSheet.Application.CellTypes;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Tests.CellTypes;

/// <summary>The choices of a dropdown cell type: tidied on the way in, and saved without losing their ids.</summary>
public sealed class CellTypeOptionsTests
{
    [Fact]
    public void Clean_TrimsAndDropsBlanks()
    {
        var cleaned = CellTypeOptions.Clean(CellKind.TextDropdown, [" Pass ", string.Empty, "  ", "Fail"]);

        Assert.Equal(["Pass", "Fail"], cleaned);
    }

    [Fact]
    public void Clean_RefusesTheSameOptionTwice_WhateverItsCase()
    {
        var problem = Assert.Throws<InvalidRequestException>(() => CellTypeOptions.Clean(CellKind.TextDropdown, ["Pass", "pass"]));

        Assert.Contains("more than once", problem.Message);
    }

    [Fact]
    public void Clean_RefusesAnOptionOverTheLongestAllowed()
    {
        Assert.Throws<InvalidRequestException>(() => CellTypeOptions.Clean(CellKind.TextDropdown, [new string('x', 101)]));
    }

    [Fact]
    public void Clean_WantsNumbersForANumberDropdown()
    {
        Assert.Equal(["1.5", "2"], CellTypeOptions.Clean(CellKind.NumberDropdown, ["1.5", "2"]));
        Assert.Throws<InvalidRequestException>(() => CellTypeOptions.Clean(CellKind.NumberDropdown, ["1.5", "two"]));
    }

    [Fact]
    public void Clean_TreatsNoOptionsAsAnEmptyList()
    {
        Assert.Empty(CellTypeOptions.Clean(CellKind.TextDropdown, null));
    }

    [Fact]
    public void Sync_KeepsTheIdOfAnOptionThatStays_SoValuesChosenOnSheetsStillPointAtIt()
    {
        var cellType = new CellType
        {
            Options =
            [
                new CellTypeOption { Id = 1, Value = "Pass", DisplayOrder = 1 },
                new CellTypeOption { Id = 2, Value = "Fail", DisplayOrder = 2 },
                new CellTypeOption { Id = 3, Value = "N/A", DisplayOrder = 3 },
            ],
        };

        CellTypeOptions.Sync(cellType, ["FAIL", "Pass", "Retest"]);

        var options = cellType.Options.OrderBy(option => option.DisplayOrder).ToList();
        Assert.Equal(["FAIL", "Pass", "Retest"], options.Select(option => option.Value));
        Assert.Equal([2, 1, 0], options.Select(option => option.Id));
        Assert.Equal([1, 2, 3], options.Select(option => option.DisplayOrder));
    }
}
