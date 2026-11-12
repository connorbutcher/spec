using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Published.Sheets;
using PUSpecSheet.Application.Published.Versions;
using PUSpecSheet.Application.Repositories.Published;

using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Tests.Database.Published;

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
        var sql = new PublishedSheetRepository(db)
            .QueryVersions(Version.SheetPublicId, PublishedVersionPoint.At(Version.PublishedAtUtc))
            .Take(1)
            .ToQueryString();

        Assert.Contains("[PublishedAtUtc] <=", sql);
        Assert.Contains("ORDER BY [s].[VersionNumber] DESC", sql);
        Assert.DoesNotContain("[Note]", sql);
    }

    [Fact]
    public void Structure_ReadsOnlyPlacementColumnsAtTheVersion()
    {
        var reader = new PublishedStructureRepository(db);

        var tables = reader.Tables(Version).ToQueryString();
        var sections = reader.Sections(Version).ToQueryString();
        var blocks = reader.ColumnBlocks(Version).ToQueryString();

        foreach (var sql in new[] { tables, sections, blocks })
        {
            Assert.Contains("[SupersededAtUtc] IS NULL OR", sql);
            Assert.Contains("[s].[SheetId] = @version_SheetId", sql);
            Assert.DoesNotContain("[RowVersion]", sql);
            Assert.DoesNotContain("[AuthorUserId]", sql);
        }
    }

    [Fact]
    public void WholeSheetCells_JoinEachCellToItsRowRevisionAtTheVersion()
    {
        var sql = new PublishedCellRepository(db).Query(Version, null, false)!.ToQueryString();

        Assert.Contains("[SheetRowRevisions]", sql);
        Assert.Contains("[IsDeleted] = CAST(0 AS bit)", sql);
        Assert.Contains("].[SheetId] = @sheetId", sql);
        Assert.DoesNotContain("[Caption]", sql);
        Assert.DoesNotContain("[CellTypes]", sql);
    }

    [Fact]
    public void Captions_AreOnlyReadWhenAskedFor()
    {
        var sql = new PublishedCellRepository(db).Query(Version, null, true)!.ToQueryString();

        Assert.Contains("[Caption]", sql);
    }

    [Fact]
    public void ACellSelection_FiltersOnTheCellIdentifiersAlone()
    {
        var scope = new PublishedSelectionScope([], [], [], [Guid.NewGuid(), Guid.NewGuid()]);

        var sql = new PublishedCellRepository(db).Query(Version, scope, false)!.ToQueryString();

        Assert.Contains("[s].[PublicId] IN", sql);
        Assert.DoesNotContain(" OR ", sql[sql.LastIndexOf("WHERE", StringComparison.Ordinal)..]);
    }

    [Fact]
    public void AMixedSelection_TakesAnythingNamedByAnyOfItsParts()
    {
        var scope = new PublishedSelectionScope([4, 5], [9, 10], [Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid(), Guid.NewGuid()]);

        var sql = new PublishedCellRepository(db).Query(Version, scope, false)!.ToQueryString();

        Assert.Contains("[SheetTableId] IN", sql);
        Assert.Contains("[SheetSectionId] IN", sql);
        Assert.Equal(2, sql.Split("[PublicId] IN").Length - 1);
    }

    [Fact]
    public void ASelectionOfNothingOnTheSheet_ReadsNothing()
    {
        var scope = new PublishedSelectionScope([], [], [], []);

        Assert.Null(new PublishedCellRepository(db).Query(Version, scope, false));
    }

    [Fact]
    public void WholeSheetValues_AreFoundThroughTheSheetsRevisionsAtTheVersion_WithNoListOfIds()
    {
        var revisions = PublishedValueRepository.RevisionsOf(db, [11, 12, 13], Version);

        var sql = PublishedValueRepository.Scope(db.NumericValues, revisions, null)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToQueryString();

        Assert.Contains("[values].[NumericValues]", sql);
        Assert.Contains("[SheetId] = @sheetId", sql);
        Assert.Contains("[IsDeleted] = CAST(0 AS bit)", sql);
        Assert.Contains("[SupersededAtUtc] > @momentUtc", sql);
        Assert.DoesNotContain("OPENJSON", sql);
        Assert.DoesNotContain("JOIN", sql);
        Assert.StartsWith("SELECT [n].[SheetCellId], [n].[Value]", sql[sql.IndexOf("SELECT", StringComparison.Ordinal)..]);
    }

    [Fact]
    public void SelectedValues_AreFoundByRevisionAndCellWithNoJoins()
    {
        var revisions = PublishedValueRepository.RevisionsOf(db, [11, 12, 12], null);

        var sql = PublishedValueRepository.Scope(db.TextValues, revisions, [101, 102])
            .Select(value => new { value.SheetCellId, value.Value })
            .ToQueryString();

        Assert.Contains("[SheetRowRevisionId] IN", sql);
        Assert.Contains("[SheetCellId] IN", sql);
        Assert.DoesNotContain("JOIN", sql);
    }

    [Fact]
    public void OptionValues_BringTheOptionTextAndItsKind()
    {
        var sql = PublishedValueRepository.Scope(db.OptionValues, PublishedValueRepository.RevisionsOf(db, [11], null), [101])
            .Select(value => new { value.SheetCellId, value.CellTypeOption.Value, value.CellTypeOption.CellType.Kind })
            .ToQueryString();

        Assert.Contains("[CellTypeOptions]", sql);
        Assert.Contains("[Kind]", sql);
    }
}
