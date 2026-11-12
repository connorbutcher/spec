using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Tests.Database.Sheets.Values;

/// <summary>
/// A whole sheet's values are read through a query over the sheet's revisions, not a long list of ids. The
/// SQL is generated without connecting to a database.
/// </summary>
public sealed class RowRevisionScopeTranslationTests : IDisposable
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
    public void AQueryScope_IsASubqueryOnTheSheet_WithNoListOfIds()
    {
        var sheetId = 7;
        var scope = RowRevisionScope.Of(db.SheetRowRevisions
            .Where(revision => revision.SheetId == sheetId)
            .CurrentAndDrafts()
            .Select(revision => revision.Id));

        var sql = scope.Apply(db.NumericValues).ToQueryString();

        Assert.Contains("[SheetId] = @sheetId", sql);
        Assert.Contains("UNION ALL", sql);
        Assert.DoesNotContain("OPENJSON", sql);
        Assert.DoesNotContain("@revisionIds", sql);
    }

    [Fact]
    public void AListScope_IsTheListOfIds()
    {
        var scope = RowRevisionScope.Of([1, 2, 3]);

        var sql = scope.Apply(db.TextValues).ToQueryString();

        Assert.Contains("IN (@listed1, @listed2, @listed3)", sql);
        Assert.False(scope.IsEmpty);
    }

    [Fact]
    public void AnEmptyList_IsEmpty()
    {
        Assert.True(RowRevisionScope.Of([]).IsEmpty);
    }
}
