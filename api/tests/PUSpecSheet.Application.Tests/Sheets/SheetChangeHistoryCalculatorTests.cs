using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Tests.Sheets;

/// <summary>
/// When each cell, row, section and column block of a sheet last changed, worked out from its published
/// revisions. Version ids are ten times their numbers here so the two can't be mixed up.
/// </summary>
public sealed class SheetChangeHistoryCalculatorTests
{
    private const int Author = 7;
    private const int Section = 100;
    private const int ParentSection = 200;

    private static readonly Dictionary<int, int> VersionNumbers = new() { [10] = 1, [20] = 2, [30] = 3 };
    private static readonly Dictionary<int, int?> SectionParents = new() { [Section] = ParentSection, [ParentSection] = null };

    private int nextRevisionId = 1;

    [Fact]
    public void ACell_LastChangedInTheVersionItsValueLastDiffered()
    {
        var first = Row(1, revisionNumber: 1, version: 1);
        var second = Row(1, revisionNumber: 2, version: 2);
        var third = Row(1, revisionNumber: 3, version: 3);
        var values = new Dictionary<int, Dictionary<int, CellValueBag>>
        {
            [first.Id] = new() { [51] = new CellValueBag { Number = 1 }, [52] = new CellValueBag { Text = "a" } },
            [second.Id] = new() { [51] = new CellValueBag { Number = 2 }, [52] = new CellValueBag { Text = "a" } },
            [third.Id] = new() { [51] = new CellValueBag { Number = 2 }, [52] = new CellValueBag { Text = "a" } },
        };

        var history = Calculate([first, second, third], values: values);

        Assert.Equal(2, history.Cells[51].VersionNumber);
        Assert.Equal(1, history.Cells[52].VersionNumber);
        Assert.Equal(2, history.Rows[1].VersionNumber);
        Assert.Equal(Author, history.Cells[51].AuthorUserId);
    }

    [Fact]
    public void ACellThatIsCleared_CountsAsChanged()
    {
        var first = Row(1, revisionNumber: 1, version: 1);
        var second = Row(1, revisionNumber: 2, version: 2);
        var values = new Dictionary<int, Dictionary<int, CellValueBag>>
        {
            [first.Id] = new() { [51] = new CellValueBag { Number = 1 } },
        };

        var history = Calculate([first, second], values: values);

        Assert.Equal(2, history.Cells[51].VersionNumber);
    }

    [Fact]
    public void ARowThatOnlyMoved_ChangesItsSectionButNotItself()
    {
        var first = Row(1, revisionNumber: 1, version: 1, displayOrder: 1024);
        var moved = Row(1, revisionNumber: 2, version: 3, displayOrder: 4096);

        var history = Calculate([first, moved]);

        Assert.Equal(1, history.Rows[1].VersionNumber);
        Assert.Equal(3, history.Sections[Section].VersionNumber);
    }

    [Fact]
    public void ASection_ShowsTheNewestChangeAmongItsRows_WhateverOrderTheyAreReadIn()
    {
        // Row 1 is the older row but moved most recently; row 2 was added in between.
        var olderRowAdded = Row(1, revisionNumber: 1, version: 1);
        var olderRowMoved = Row(1, revisionNumber: 2, version: 3, displayOrder: 4096);
        var newerRowAdded = Row(2, revisionNumber: 1, version: 2);

        var inOrder = Calculate([olderRowAdded, olderRowMoved, newerRowAdded]);
        var reversed = Calculate([newerRowAdded, olderRowMoved, olderRowAdded]);

        Assert.Equal(3, inOrder.Sections[Section].VersionNumber);
        Assert.Equal(3, reversed.Sections[Section].VersionNumber);
    }

    [Fact]
    public void ASubSectionChange_MarksItsParent_ButNeverOverAnyNewerChange()
    {
        var rowAddedToParent = Row(9, revisionNumber: 1, version: 3);
        var subSection = new SheetSectionRevision
        {
            Id = nextRevisionId++,
            SheetSectionId = Section,
            Status = RevisionStatus.Published,
            SheetVersionId = 20,
            PublishedAtUtc = At(2),
            AuthorUserId = Author,
        };
        var rowSections = new Dictionary<int, int> { [9] = ParentSection };

        var history = Calculate([rowAddedToParent], sections: [subSection], rowSections: rowSections);

        Assert.Equal(3, history.Sections[ParentSection].VersionNumber);
        Assert.DoesNotContain(Section, history.Sections.Keys);
    }

    [Fact]
    public void AColumnBlock_LastChangedInItsNewestPublishedRevision()
    {
        var added = Block(40, version: 1);
        var moved = Block(40, version: 3);

        var history = Calculate([], columnBlocks: [moved, added]);

        Assert.Equal(3, history.ColumnBlocks[40].VersionNumber);
        Assert.Equal(DateTimeKind.Utc, history.ColumnBlocks[40].AtUtc.Kind);
    }

    [Fact]
    public void ARevisionWithoutAKnownVersion_IsLeftOut()
    {
        var orphan = Row(1, revisionNumber: 1, version: 1);
        orphan.SheetVersionId = 999;

        var history = Calculate([orphan]);

        Assert.Equal(0, history.Count);
    }

    private static DateTime At(int version)
    {
        return new DateTime(2026, 10, version, 9, 0, 0, DateTimeKind.Unspecified);
    }

    private static SheetChangeHistory Calculate(
        IReadOnlyList<SheetRowRevision> rows,
        IReadOnlyList<SheetSectionRevision>? sections = null,
        IReadOnlyList<SheetColumnBlockRevision>? columnBlocks = null,
        Dictionary<int, Dictionary<int, CellValueBag>>? values = null,
        Dictionary<int, int>? rowSections = null)
    {
        return SheetChangeHistoryCalculator.Calculate(
            rows,
            sections ?? [],
            columnBlocks ?? [],
            values ?? [],
            VersionNumbers,
            rowSections ?? rows.Select(row => row.SheetRowId).Distinct().ToDictionary(id => id, _ => Section),
            SectionParents);
    }

    private SheetRowRevision Row(int rowId, int revisionNumber, int version, int displayOrder = 1024)
    {
        return new SheetRowRevision
        {
            Id = nextRevisionId++,
            SheetRowId = rowId,
            RevisionNumber = revisionNumber,
            Status = RevisionStatus.Published,
            DisplayOrder = displayOrder,
            SheetVersionId = version * 10,
            PublishedAtUtc = At(version),
            AuthorUserId = Author,
        };
    }

    private SheetColumnBlockRevision Block(int blockId, int version)
    {
        return new SheetColumnBlockRevision
        {
            Id = nextRevisionId++,
            SheetColumnBlockId = blockId,
            Status = RevisionStatus.Published,
            SheetVersionId = version * 10,
            PublishedAtUtc = At(version),
            AuthorUserId = Author,
        };
    }
}
