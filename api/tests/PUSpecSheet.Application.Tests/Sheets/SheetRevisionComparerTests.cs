using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Tests.Sheets;

/// <summary>What counts as a draft that differs from what is published, for order, removal and titles.</summary>
public sealed class SheetRevisionComparerTests
{
    private static SheetRowRevision Revision(int order, bool deleted = false)
    {
        return new SheetRowRevision { DisplayOrder = order, IsDeleted = deleted };
    }

    [Fact]
    public void ARevisionAtTheSamePlace_IsUnchanged()
    {
        Assert.True(SheetRevisionComparer.SameStructure(Revision(2048), Revision(2048)));
    }

    [Fact]
    public void AMovedOrRemovedRevision_IsChanged()
    {
        Assert.False(SheetRevisionComparer.SameStructure(Revision(1024), Revision(2048)));
        Assert.False(SheetRevisionComparer.SameStructure(Revision(2048, deleted: true), Revision(2048)));
    }

    [Theory]
    [InlineData("Valve limits", "  Valve limits ", true)]
    [InlineData(null, "", true)]
    [InlineData("Valve limits", "Valve limit", false)]
    [InlineData("Valve limits", null, false)]
    public void ATitleRestoredOrRetyped_IsComparedWithoutSpacing(string? published, string? draft, bool same)
    {
        Assert.Equal(same, SheetRevisionComparer.SameText(published, draft));
    }
}
