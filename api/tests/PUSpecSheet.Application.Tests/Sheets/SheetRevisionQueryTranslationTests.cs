using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Tests.Sheets;

/// <summary>
/// The live-revision queries turn into SQL the filtered unique indexes can answer. The SQL is generated
/// without connecting to a database, so these fail if a query stops matching those indexes and goes back
/// to reading every revision an item has ever had.
/// </summary>
public sealed class SheetRevisionQueryTranslationTests : IDisposable
{
    private readonly PuSpecSheetDbContext db = new(
        new DbContextOptionsBuilder<PuSpecSheetDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options);

    public void Dispose()
    {
        db.Dispose();
    }

    [Fact]
    public void CurrentAndDrafts_IsTheTwoIndexedCases_NotOnePredicateOverEveryRevision()
    {
        var sql = db.SheetRowRevisions.Where(revision => revision.SheetRowId == 5).CurrentAndDrafts().ToQueryString();

        Assert.Contains("[Status] = 1 AND [s].[SupersededAtUtc] IS NULL", sql);
        Assert.Contains("UNION ALL", sql);
        Assert.Contains("[Status] = 0", sql);
    }

    [Fact]
    public void VisibleTo_TakesOnlyThatUsersDrafts()
    {
        var sql = db.SheetSectionRevisions.Where(revision => revision.SheetSectionId == 5).VisibleTo(42).ToQueryString();

        Assert.Contains("[Status] = 1 AND [s].[SupersededAtUtc] IS NULL", sql);
        Assert.Contains("UNION ALL", sql);
        Assert.Contains("[Status] = 0 AND [s0].[AuthorUserId] = @userId", sql);
    }

    [Fact]
    public void Current_MatchesTheOneCurrentRevisionIndex()
    {
        var sql = db.SheetTableRevisions.Current().ToQueryString();

        Assert.Contains("[Status] = 1 AND [s].[SupersededAtUtc] IS NULL", sql);
        Assert.DoesNotContain("UNION", sql);
    }

    [Fact]
    public void DraftsInAPublishOfMyOwn_AreOnlyMine()
    {
        var sql = db.SheetRowRevisions.DraftsIn(PublishScope.Mine, 42).ToQueryString();

        Assert.Contains("[Status] = 0 AND [s].[AuthorUserId] = @userId", sql);
    }

    [Fact]
    public void DraftsInAPublishOfEverything_AreEveryonesDrafts_AndNothingPublished()
    {
        var sql = db.SheetRowRevisions.DraftsIn(PublishScope.All, 42).ToQueryString();

        Assert.Contains("[Status] = 0", sql);
        Assert.DoesNotContain("AuthorUserId] =", sql);
    }

    [Theory]
    [InlineData(3, "IN (@ids1, @ids2, @ids3)")]
    [InlineData(5000, "OPENJSON(@ids)")]
    public void ItemIds_ArePassedAsParameters_HoweverManyThereAre(int count, string expected)
    {
        var ids = Enumerable.Range(1, count).ToList();

        var sql = db.SheetRowRevisions.Where(revision => ids.Contains(revision.SheetRowId)).Current().ToQueryString();

        Assert.Contains(expected, sql);
    }

    [Fact]
    public void RevisionsOfASheet_AreReachedThroughTheirItems()
    {
        var sql = db.RowRevisionsOf(7).DraftsOf(42).ToQueryString();

        Assert.Contains("[SheetId] = @sheetId", sql);
        Assert.Contains("[Status] = 0 AND [s].[AuthorUserId] = @userId", sql);
    }
}
