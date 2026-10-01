using Microsoft.EntityFrameworkCore.Metadata;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Tests.Sheets;

/// <summary>
/// Every sheet, table, section, row and cell has one public identifier that is created with it and can
/// never change. Revisions are separate records pointing at the item, so publishing, moving or editing
/// only adds revisions and the item's identifier is the same in every version.
/// </summary>
public sealed class SheetPublicIdTests
{
    public static TheoryData<Type> Items => new()
    {
        typeof(Sheet),
        typeof(SheetTable),
        typeof(SheetSection),
        typeof(SheetRow),
        typeof(SheetCell),
    };

    public static TheoryData<Type> Revisions => new()
    {
        typeof(SheetTableRevision),
        typeof(SheetSectionRevision),
        typeof(SheetRowRevision),
    };

    [Theory]
    [MemberData(nameof(Items))]
    public void PublicId_IsGeneratedOnInsertAndNeverChanged(Type item)
    {
        var property = SheetModelFixture.Model.FindEntityType(item)!.FindProperty(nameof(SheetRow.PublicId))!;

        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        Assert.Equal(PropertySaveBehavior.Throw, property.GetAfterSaveBehavior());
    }

    [Theory]
    [MemberData(nameof(Items))]
    public void PublicId_IsUnique(Type item)
    {
        var entity = SheetModelFixture.Model.FindEntityType(item)!;
        var property = entity.FindProperty(nameof(SheetRow.PublicId))!;

        Assert.Contains(entity.GetIndexes(), index => index.IsUnique && index.Properties.SequenceEqual([property]));
    }

    [Theory]
    [MemberData(nameof(Revisions))]
    public void Revisions_CannotMintAnIdentityOfTheirOwn(Type revision)
    {
        Assert.Null(SheetModelFixture.Model.FindEntityType(revision)!.FindProperty(nameof(SheetRow.PublicId)));
    }

    [Theory]
    [InlineData(typeof(SheetTableRevision), nameof(SheetTableRevision.SheetTableId))]
    [InlineData(typeof(SheetSectionRevision), nameof(SheetSectionRevision.SheetSectionId))]
    [InlineData(typeof(SheetRowRevision), nameof(SheetRowRevision.SheetRowId))]
    public void Revisions_PointAtTheItemTheyAreAVersionOf(Type revision, string itemKey)
    {
        var entity = SheetModelFixture.Model.FindEntityType(revision)!;

        Assert.Contains(entity.GetForeignKeys(), key => key.Properties.Any(property => property.Name == itemKey));
    }
}
