using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Tests.Published;

/// <summary>
/// The published queries turn into SQL that reads only what a caller needs. The SQL is generated without
/// connecting to a database, so these fail if a query can't be translated or starts pulling extra columns.
/// </summary>
public sealed class PublishedQueryTranslationTests : IDisposable
{
    private static readonly ResolvedSheetVersion Version = new(
        7,
        Guid.Parse("6f1c0000-0000-0000-0000-000000000001"),
        3,
        new DateTime(2026, 9, 30, 14, 2, 11, DateTimeKind.Utc));

    private readonly PuSpecSheetDbContext db = new(
        new DbContextOptionsBuilder<PuSpecSheetDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options);

    public void Dispose()
    {
        db.Dispose();
    }

    [Fact]
    public void ResolvingAMoment_TakesTheNewestVersionAtOrBeforeIt()
    {
        var sql = new PublishedVersionResolver(db)
            .Query(Version.SheetPublicId, PublishedVersionPoint.At(Version.PublishedAtUtc))
            .Select(version => new { version.SheetId, version.VersionNumber, version.PublishedAtUtc })
            .Take(1)
            .ToQueryString();

        Assert.Contains("[PublishedAtUtc] <=", sql);
        Assert.Contains("ORDER BY [s].[VersionNumber] DESC", sql);
        Assert.DoesNotContain("[Note]", sql);
    }

    [Fact]
    public void Structure_ReadsOnlyPlacementColumnsAtTheVersion()
    {
        var reader = new PublishedStructureReader(db);

        var tables = reader.Tables(Version).ToQueryString();
        var sections = reader.Sections(Version).ToQueryString();
        var blocks = reader.ColumnBlocks(Version).ToQueryString();

        foreach (var sql in new[] { tables, sections, blocks })
        {
            Assert.Contains("[SupersededAtUtc] IS NULL OR", sql);
            Assert.DoesNotContain("[RowVersion]", sql);
            Assert.DoesNotContain("[AuthorUserId]", sql);
        }
    }

    [Fact]
    public void WholeSheetCells_JoinEachCellToItsRowRevisionAtTheVersion()
    {
        var sql = new PublishedCellReader(db).Query(Version, null, false)!.ToQueryString();

        Assert.Contains("[SheetRowRevisions]", sql);
        Assert.Contains("[IsDeleted] = CAST(0 AS bit)", sql);
        Assert.DoesNotContain("[Caption]", sql);
        Assert.DoesNotContain("[CellTypes]", sql);
    }

    [Fact]
    public void Captions_AreOnlyReadWhenAskedFor()
    {
        var sql = new PublishedCellReader(db).Query(Version, null, true)!.ToQueryString();

        Assert.Contains("[Caption]", sql);
    }

    [Fact]
    public void ACellSelection_FiltersOnTheCellIdentifiersAlone()
    {
        var scope = new PublishedSelectionScope([], [], [], [Guid.NewGuid(), Guid.NewGuid()]);

        var sql = new PublishedCellReader(db).Query(Version, scope, false)!.ToQueryString();

        Assert.Contains("[s].[PublicId] IN", sql);
        Assert.DoesNotContain(" OR ", sql[sql.LastIndexOf("WHERE", StringComparison.Ordinal)..]);
    }

    [Fact]
    public void AMixedSelection_TakesAnythingNamedByAnyOfItsParts()
    {
        var scope = new PublishedSelectionScope([4, 5], [9, 10], [Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid(), Guid.NewGuid()]);

        var sql = new PublishedCellReader(db).Query(Version, scope, false)!.ToQueryString();

        Assert.Contains("[SheetTableId] IN", sql);
        Assert.Contains("[SheetSectionId] IN", sql);
        Assert.Equal(2, sql.Split("[PublicId] IN").Length - 1);
    }

    [Fact]
    public void ASelectionOfNothingOnTheSheet_ReadsNothing()
    {
        var scope = new PublishedSelectionScope([], [], [], []);

        Assert.Null(new PublishedCellReader(db).Query(Version, scope, false));
    }

    [Fact]
    public void WholeSheetValues_AreFoundThroughTheRowRevisionsAtTheVersion()
    {
        var sql = PublishedValueReader.Scope(db.NumericValues, Version, null, null)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToQueryString();

        Assert.Contains("[values].[NumericValues]", sql);
        Assert.Contains("[SheetRowRevisions]", sql);
        Assert.StartsWith("SELECT [n].[SheetCellId], [n].[Value]", sql[sql.IndexOf("SELECT", StringComparison.Ordinal)..]);
    }

    [Fact]
    public void SelectedValues_AreFoundByRevisionAndCellWithNoJoins()
    {
        var sql = PublishedValueReader.Scope(db.TextValues, Version, [11, 12], [101, 102])
            .Select(value => new { value.SheetCellId, value.Value })
            .ToQueryString();

        Assert.Contains("[SheetRowRevisionId] IN", sql);
        Assert.Contains("[SheetCellId] IN", sql);
        Assert.DoesNotContain("JOIN", sql);
    }

    [Fact]
    public void OptionValues_BringTheOptionTextAndItsKind()
    {
        var sql = PublishedValueReader.Scope(db.OptionValues, Version, [11], [101])
            .Select(value => new { value.SheetCellId, value.CellTypeOption.Value, value.CellTypeOption.CellType.Kind })
            .ToQueryString();

        Assert.Contains("[CellTypeOptions]", sql);
        Assert.Contains("[Kind]", sql);
    }
}
