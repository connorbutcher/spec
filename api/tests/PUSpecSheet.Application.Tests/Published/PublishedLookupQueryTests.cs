using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Tests.Published;

/// <summary>
/// A lookup's question, the SQL it turns into (generated without connecting to a database) and the key
/// that names its answer.
/// </summary>
public sealed class PublishedLookupQueryTests : IDisposable
{
    private static readonly DateTime Moment = new(2026, 10, 4, 18, 0, 0, DateTimeKind.Utc);

    private static readonly ResolvedSheetVersion Version = new(7, Guid.Parse("6f1c0000-0000-0000-0000-000000000001"), 2, Moment);

    private readonly PuSpecSheetDbContext db = new(
        new DbContextOptionsBuilder<PuSpecSheetDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options);

    public void Dispose()
    {
        db.Dispose();
    }

    [Fact]
    public void FindingAValue_SeeksTheIndexedStartOfTheValueAndChecksTheKeyAndTheMoment()
    {
        var sql = new PublishedLookupHitReader(db)
            .Query(PublishedLookupCriteria.Create("partNumber", "P-1003"), Moment, null)
            .ToQueryString();

        Assert.Contains("[LookupValue] =", sql);
        Assert.Contains("[LookupKey] =", sql);
        Assert.Contains("[SupersededAtUtc] IS NULL OR", sql);
        Assert.Contains("[IsDeleted] = CAST(0 AS bit)", sql);
        Assert.DoesNotContain("[Phases]", sql[sql.LastIndexOf("WHERE", StringComparison.Ordinal)..]);
    }

    [Fact]
    public void FindingAValue_CanKeepToASheetAPhaseAndASheetType()
    {
        var sql = new PublishedLookupHitReader(db)
            .Query(PublishedLookupCriteria.Create("partNumber", "P-1003", "V6", 3), Moment, 7)
            .ToQueryString();

        var where = sql[sql.LastIndexOf("WHERE", StringComparison.Ordinal)..];
        Assert.Contains("[SheetId] =", where);
        Assert.Contains("[Code] =", where);
        Assert.Contains("[SheetTypeId] =", where);
    }

    [Fact]
    public void AMatchInAColumnBlock_ReadsThatBlockAndTheRowsOwnCellsOnly()
    {
        var hit = new PublishedLookupHit(7, Version.SheetPublicId, "V6", 3, 4, 10, 1, 2, 6, true);

        var sql = new PublishedLookupCellReader(db).Query(Version, hit, null).ToQueryString();

        Assert.Contains("[SheetColumnBlockId] IS NULL OR", sql);
        Assert.Contains("[SheetRowRevisions]", sql);
        Assert.DoesNotContain("[StyleOverride]", sql);
        Assert.DoesNotContain("[ConfigurationOverride]", sql);
    }

    [Fact]
    public void AMatchInARow_ReadsItsSectionsAndTheHeader()
    {
        var hit = new PublishedLookupHit(7, Version.SheetPublicId, "V6", 1, 4, 50, 11, 23, null, false);

        var sql = new PublishedLookupCellReader(db).Query(Version, hit, [50, 51]).ToQueryString();

        Assert.Contains("[SheetSectionId] IN", sql);
        Assert.Contains("[Role] =", sql);
    }

    [Theory]
    [InlineData(null, "P-1003")]
    [InlineData("part number", "P-1003")]
    [InlineData("1part", "P-1003")]
    [InlineData("partNumber", " ")]
    public void AQuestionWithoutAKeyOrAValue_IsRefused(string? key, string? value)
    {
        Assert.Throws<InvalidRequestException>(() => PublishedLookupCriteria.Create(key, value));
    }

    [Fact]
    public void AQuestion_IsTrimmed()
    {
        var criteria = PublishedLookupCriteria.Create(" partNumber ", " P-1003 ", " V6 ");

        Assert.Equal("partNumber", criteria.Key);
        Assert.Equal("P-1003", criteria.Value);
        Assert.Equal("V6", criteria.PhaseCode);
    }

    [Fact]
    public void TheAnswersKey_ChangesWithTheCellsFoundAndTheVersionsRead()
    {
        var criteria = PublishedLookupCriteria.Create("partNumber", "P-1003");
        var first = new PublishedLookupHit(7, Version.SheetPublicId, "V6", 3, 4, 10, 1, 2, 6, true);
        var second = first with { CellId = 9 };
        var versions = new Dictionary<int, ResolvedSheetVersion> { [7] = Version };
        var newer = new Dictionary<int, ResolvedSheetVersion> { [7] = Version with { VersionNumber = 3 } };

        var key = new PublishedLookupResolution(criteria, [first, second], versions).Key;

        Assert.Equal(key, new PublishedLookupResolution(criteria, [second, first], versions).Key);
        Assert.NotEqual(key, new PublishedLookupResolution(criteria, [first], versions).Key);
        Assert.NotEqual(key, new PublishedLookupResolution(criteria, [first, second], newer).Key);
        Assert.NotEqual(key, new PublishedLookupResolution(PublishedLookupCriteria.Create("partNumber", "P-1004"), [first, second], versions).Key);
    }
}
