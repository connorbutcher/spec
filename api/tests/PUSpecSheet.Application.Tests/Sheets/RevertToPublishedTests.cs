using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Tests.Sheets;

/// <summary>
/// Every kind of change a person can make to a sheet, with what putting it back looks like. Each row says
/// whether the draft that results is the same as what is published, which is what releases the lock and
/// removes the need to publish. Operations that can't be put back (removing something that was published)
/// or that never create a draft that outlives the edit (adding then removing something never published,
/// which is deleted outright) are listed in the comments instead, and are checked against a real database.
/// </summary>
public sealed class RevertToPublishedTests
{
    private static Dictionary<int, CellValueBag> One(CellValueBag bag)
    {
        return new Dictionary<int, CellValueBag> { [1] = bag };
    }

    /// <summary>Each cell kind: a value is set, then set back to what was published.</summary>
    public static TheoryData<string, CellValueBag?, CellValueBag?> CellValueReverts => new()
    {
        { "Text", new CellValueBag { Text = "Bore" }, new CellValueBag { Text = "Bore " } },
        { "Text cleared and refilled", new CellValueBag { Text = "Bore" }, new CellValueBag { Text = " Bore" } },
        { "Text empty versus none", null, new CellValueBag { Text = string.Empty } },
        { "Number", new CellValueBag { Number = 82.01m }, new CellValueBag { Number = 82.010m } },
        { "Number cleared and not set", null, new CellValueBag() },
        { "Date", new CellValueBag { Date = new DateOnly(2026, 10, 1) }, new CellValueBag { Date = new DateOnly(2026, 10, 1) } },
        { "Checkbox ticked", new CellValueBag { Boolean = true }, new CellValueBag { Boolean = true } },
        { "Checkbox unticked versus none", null, new CellValueBag { Boolean = false } },
        { "Text dropdown", new CellValueBag { OptionId = 7 }, new CellValueBag { OptionId = 7 } },
        { "Number dropdown", new CellValueBag { OptionId = 9 }, new CellValueBag { OptionId = 9 } },
    };

    [Theory]
    [MemberData(nameof(CellValueReverts))]
    public void EditingACellValueBackToWhatWasPublished_LeavesNothingToPublish(string kind, CellValueBag? published, CellValueBag? restored)
    {
        Assert.NotEmpty(kind);
        var before = published is null ? new Dictionary<int, CellValueBag>() : One(published);
        var after = restored is null ? new Dictionary<int, CellValueBag>() : One(restored);

        Assert.True(CellValuesComparer.Same(before, after));
    }

    /// <summary>Anything with a place in an order: rows, sections, tables and column blocks.</summary>
    public static TheoryData<string, ISheetRevision, ISheetRevision> RevisionsOfEveryKind => new()
    {
        { "Row", new SheetRowRevision { DisplayOrder = 2048 }, new SheetRowRevision { DisplayOrder = 2048 } },
        { "Section", new SheetSectionRevision { DisplayOrder = 2048 }, new SheetSectionRevision { DisplayOrder = 2048 } },
        { "Table", new SheetTableRevision { DisplayOrder = 2048 }, new SheetTableRevision { DisplayOrder = 2048 } },
        { "Column block", new SheetColumnBlockRevision { DisplayOrder = 2048 }, new SheetColumnBlockRevision { DisplayOrder = 2048 } },
    };

    [Theory]
    [MemberData(nameof(RevisionsOfEveryKind))]
    public void AnItemMovedAwayAndBack_IsBackWhereItWasPublished(string kind, ISheetRevision published, ISheetRevision draft)
    {
        Assert.NotEmpty(kind);
        int[] others = [1024, 3072];

        // Away to the end, then back to the middle: the middle item gets its published order again.
        var away = OrderGaps.PlaceAt(others, position: 3, publishedOrder: published.DisplayOrder);
        Assert.NotEqual(published.DisplayOrder, away);
        draft.DisplayOrder = OrderGaps.PlaceAt(others, position: 2, publishedOrder: published.DisplayOrder);

        Assert.True(SheetRevisionComparer.SameStructure(draft, published));
    }

    [Fact]
    public void SeveralItemsMovedAndThenRestoredInADifferentWay_AreInTheirPublishedOrder()
    {
        // A, B, C published at 1024, 2048, 3072. A to the end, then B to the end, then C to the end:
        // every number is new but the order A, B, C is what was published.
        var items = new (int Id, int? Published, int Shown, bool Removed)[]
        {
            (1, 1024, 4096, false),
            (2, 2048, 5120, false),
            (3, 3072, 6144, false),
        };

        Assert.True(OrderRestoration.IsRestored(items));
    }

    [Fact]
    public void SeveralItemsLeftInADifferentOrder_AreStillChanged()
    {
        var items = new (int Id, int? Published, int Shown, bool Removed)[]
        {
            (1, 1024, 4096, false),
            (2, 2048, 2048, false),
            (3, 3072, 3072, false),
        };

        Assert.False(OrderRestoration.IsRestored(items));
    }

    [Fact]
    public void ARemovedItem_DoesNotStopTheRestWithoutItBeingRestored()
    {
        var items = new (int Id, int? Published, int Shown, bool Removed)[]
        {
            (1, 1024, 4096, false),
            (2, 2048, 2048, true),
            (3, 3072, 6144, false),
        };

        Assert.True(OrderRestoration.IsRestored(items));
    }

    [Fact]
    public void AnItemNeverPublished_HasNoOriginalPlaceSoTheNumbersStay()
    {
        var items = new (int Id, int? Published, int Shown, bool Removed)[]
        {
            (1, 1024, 4096, false),
            (2, null, 2048, false),
        };

        Assert.False(OrderRestoration.IsRestored(items));
    }

    [Fact]
    public void ATableTitleChangedAndRestored_IsTheSame()
    {
        Assert.True(SheetRevisionComparer.SameText("Valve limits", " Valve limits"));
        Assert.True(SheetRevisionComparer.SameText(null, "  "));
        Assert.False(SheetRevisionComparer.SameText("Valve limits", "Valve limits 2"));
    }

    // Operation by operation, what happens when it is put back (see also the live checks in the thread):
    //   Row, section, table and column block add, then remove before publishing: the item only ever existed as
    //     its author's draft, so removing it deletes it outright. No draft is left. (Not testable without a database.)
    //   Row, section, table and column block remove of something published: a removal can't be put back, so it
    //     stays a change until published or until the sheet's drafts are discarded.
    //   Cell values (Text, Number, Date, Checkbox, Text dropdown, Number dropdown): the cases above.
    //   Row, section, table and column block moves, away and back or in a longer round trip: the cases above.
    //   Table title: the title case above.
    //   Column block add: new column blocks are first drafts, covered with add then remove.
}
