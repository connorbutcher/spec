using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Collaboration;

/// <summary>The signature must change exactly when something other people can see does.</summary>
public sealed class SheetLockSignatureTests
{
    [Fact]
    public void TheSameSheet_SeenByTwoPeople_HasTheSameSignature()
    {
        var asHolder = Sheet(3, Row(10, new SheetLock { UserId = 1, UserName = "Developer", IsMine = true }));
        var asOther = Sheet(3, Row(10, new SheetLock { UserId = 1, UserName = "Developer", IsMine = false }));

        Assert.Equal(SheetLockSignature.Of(asHolder), SheetLockSignature.Of(asOther));
    }

    [Fact]
    public void CheckingARowOut_ChangesTheSignature()
    {
        var free = Sheet(3, Row(10, null));
        var held = Sheet(3, Row(10, new SheetLock { UserId = 1, UserName = "Developer", IsMine = true }));

        Assert.NotEqual(SheetLockSignature.Of(free), SheetLockSignature.Of(held));
    }

    [Fact]
    public void HandingARowToSomeoneElse_ChangesTheSignature()
    {
        var mine = Sheet(3, Row(10, new SheetLock { UserId = 1, UserName = "Developer", IsMine = true }));
        var theirs = Sheet(3, Row(10, new SheetLock { UserId = 2, UserName = "Engineer Two", IsMine = false }));

        Assert.NotEqual(SheetLockSignature.Of(mine), SheetLockSignature.Of(theirs));
    }

    [Fact]
    public void Publishing_ChangesTheSignature()
    {
        Assert.NotEqual(SheetLockSignature.Of(Sheet(3, Row(10, null))), SheetLockSignature.Of(Sheet(4, Row(10, null))));
    }

    [Fact]
    public void ARowNobodyElseCanSeeYet_IsLeftOut()
    {
        var without = Sheet(3, Row(10, null));
        var withPending = Sheet(3, Row(10, null), Row(11, new SheetLock
        {
            UserId = 1,
            UserName = "Developer",
            IsMine = true,
        }, isPending: true));

        Assert.Equal(SheetLockSignature.Of(without), SheetLockSignature.Of(withPending));
    }

    [Fact]
    public void WhenOnlyRowsChangeHands_TheyAreNamed()
    {
        var before = SheetLockSignature.Entries(Sheet(3, Row(10, null), Row(11, new SheetLock
        {
            UserId = 1,
            UserName = "Developer",
            IsMine = true,
        }), Row(12, null)));
        var after = SheetLockSignature.Entries(Sheet(3, Row(10, new SheetLock
        {
            UserId = 2,
            UserName = "Engineer Two",
            IsMine = false,
        }), Row(11, null), Row(12, null)));

        Assert.Equal([10, 11], SheetLockSignature.ChangedRows(before, after));
    }

    [Fact]
    public void WhenAVersionIsPublished_NoRowsAreNamed()
    {
        var before = SheetLockSignature.Entries(Sheet(3, Row(10, new SheetLock { UserId = 1, UserName = "Developer", IsMine = true })));
        var after = SheetLockSignature.Entries(Sheet(4, Row(10, null)));

        Assert.Null(SheetLockSignature.ChangedRows(before, after));
    }

    private static SheetRow Row(int id, SheetLock? held, bool isPending = false)
    {
        return new SheetRow
        {
            Id = id,
            PublicId = Guid.Empty,
            TemplateRowId = 1,
            DisplayOrder = id,
            Lock = held,
            IsPending = isPending,
            CanRemove = true,
            Cells = [],
        };
    }

    private static Sheet Sheet(int latestVersion, params SheetRow[] rows)
    {
        var section = new SheetSection
        {
            Id = 1,
            PublicId = Guid.Empty,
            TemplateSectionId = 1,
            Name = "Limits",
            Role = SectionRole.Header,
            MinInstances = 1,
            MaxInstances = 1,
            InitialInstances = 1,
            DisplayOrder = 1,
            IsPending = false,
            CanRemove = false,
            Rows = rows,
            Sections = [],
            AddableSections = [],
        };
        var table = new SheetTable
        {
            Id = 1,
            PublicId = Guid.Empty,
            TableTemplateId = 1,
            TemplateName = "Limits",
            TemplateVersionNumber = 1,
            Orientation = TemplateOrientation.Vertical,
            StickyColumnCount = 0,
            DisplayOrder = 1,
            IsPending = false,
            Sections = [section],
            AddableSections = [],
            ColumnBlocks = [],
            AddableColumnBlocks = [],
        };
        return new Sheet
        {
            Id = 1,
            PublicId = Guid.Empty,
            PhaseId = 1,
            SheetTypeId = 1,
            IsLive = true,
            LatestVersionNumber = latestVersion,
            MyDraftCount = 0,
            Versions = [],
            Tables = [table],
            AvailableTemplates = [],
        };
    }
}
