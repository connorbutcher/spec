using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

/// <summary>The signature must change exactly when something other people can see does.</summary>
public sealed class SheetLockSignatureTests
{
    [Fact]
    public void TheSameSheet_SeenByTwoPeople_HasTheSameSignature()
    {
        var asHolder = Sheet(3, Row(10, new SheetLockDto(1, "Developer", IsMine: true)));
        var asOther = Sheet(3, Row(10, new SheetLockDto(1, "Developer", IsMine: false)));

        Assert.Equal(SheetLockSignature.Of(asHolder), SheetLockSignature.Of(asOther));
    }

    [Fact]
    public void CheckingARowOut_ChangesTheSignature()
    {
        var free = Sheet(3, Row(10, null));
        var held = Sheet(3, Row(10, new SheetLockDto(1, "Developer", IsMine: true)));

        Assert.NotEqual(SheetLockSignature.Of(free), SheetLockSignature.Of(held));
    }

    [Fact]
    public void HandingARowToSomeoneElse_ChangesTheSignature()
    {
        var mine = Sheet(3, Row(10, new SheetLockDto(1, "Developer", IsMine: true)));
        var theirs = Sheet(3, Row(10, new SheetLockDto(2, "Engineer Two", IsMine: false)));

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
        var withPending = Sheet(3, Row(10, null), Row(11, new SheetLockDto(1, "Developer", IsMine: true), isPending: true));

        Assert.Equal(SheetLockSignature.Of(without), SheetLockSignature.Of(withPending));
    }

    private static SheetRowDto Row(int id, SheetLockDto? held, bool isPending = false)
    {
        return new SheetRowDto(id, Guid.Empty, 1, id, held, isPending, CanRemove: true, [], null);
    }

    private static SheetDto Sheet(int latestVersion, params SheetRowDto[] rows)
    {
        var section = new SheetSectionDto(1, Guid.Empty, 1, "Limits", SectionRole.Header, 1, 1, 1, 1, null, false, false, rows, [], [], [], null);
        var table = new SheetTableDto(1, Guid.Empty, 1, "Limits", 1, TemplateOrientation.Vertical, 0, null, 1, null, false, [section], [], [], []);
        return new SheetDto(1, Guid.Empty, 1, 1, true, null, null, latestVersion, 0, [], [table], []);
    }
}
