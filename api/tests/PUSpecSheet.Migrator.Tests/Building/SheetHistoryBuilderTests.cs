using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Migrator.Building;
using PUSpecSheet.Migrator.Legacy;

namespace PUSpecSheet.Migrator.Tests.Building;

public sealed class SheetHistoryBuilderTests
{
    private static readonly DateTime _monday = new(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _tuesday = _monday.AddDays(1);
    private static readonly DateTime _wednesday = _monday.AddDays(2);
    private static readonly DateTime _now = _monday.AddDays(30);

    private readonly LegacySheetFactory _legacy = new();

    [Fact]
    public void EachCopy_BecomesAVersion()
    {
        var built = Build(
            _legacy.Copy(1, _monday, "a"),
            _legacy.Copy(2, _tuesday, "a"));

        int[] numbers = [1, 2];
        DateTime[] times = [_monday, _tuesday];
        Assert.Equal(numbers, built.Sheet.Versions.Select(version => version.VersionNumber));
        Assert.Equal(times, built.Sheet.Versions.Select(version => version.PublishedAtUtc));
        Assert.All(built.Sheet.Versions, version => Assert.Equal(LegacySheetFactory.UserId, version.PublishedByUserId));
    }

    [Fact]
    public void AnUnchangedRow_HasOneRevisionForEveryVersion()
    {
        var built = Build(
            _legacy.Copy(1, _monday, "a"),
            _legacy.Copy(2, _tuesday, "a"),
            _legacy.Copy(3, _wednesday, "a"));

        var row = Assert.Single(Rows(built));
        var revision = Assert.Single(row.Revisions);
        Assert.Equal(_monday, revision.PublishedAtUtc);
        Assert.Null(revision.SupersededAtUtc);
        Assert.Equal("a", Assert.Single(revision.TextValues).Value);
    }

    [Fact]
    public void AChangedRow_GetsARevisionThatReplacesTheLastAtTheSameMoment()
    {
        var built = Build(
            _legacy.Copy(1, _monday, "a"),
            _legacy.Copy(2, _tuesday, "b"));

        var row = Assert.Single(Rows(built));
        var revisions = row.Revisions.OrderBy(revision => revision.RevisionNumber).ToList();
        Assert.Equal(2, revisions.Count);
        Assert.Equal(_tuesday, revisions[0].SupersededAtUtc);
        Assert.Equal(_tuesday, revisions[1].PublishedAtUtc);
        Assert.Null(revisions[1].SupersededAtUtc);
        Assert.Equal(2, revisions[1].SheetVersion!.VersionNumber);
        Assert.Equal("b", Assert.Single(revisions[1].TextValues).Value);
    }

    [Fact]
    public void ARowAddedInTheMiddle_LeavesTheRowsAroundItAlone()
    {
        var built = Build(
            _legacy.Copy(1, _monday, "a", "c"),
            _legacy.Copy(2, _tuesday, "a", "b", "c"));

        var rows = Rows(built);
        Assert.Equal(3, rows.Count);

        // Only "b" is new; "a" and "c" keep their orders, so neither gets another revision.
        Assert.All(rows, row => Assert.Single(row.Revisions));
        var orders = rows
            .ToDictionary(row => row.Revisions.Single().TextValues.Single().Value, row => row.Revisions.Single().DisplayOrder);
        Assert.True(orders["a"] < orders["b"] && orders["b"] < orders["c"]);
    }

    [Fact]
    public void ARowMissingFromACopy_IsRemovedKeepingItsValues()
    {
        var built = Build(
            _legacy.Copy(1, _monday, "a", "b"),
            _legacy.Copy(2, _tuesday, "a"));

        var removed = Rows(built)
            .SelectMany(row => row.Revisions)
            .Single(revision => revision.IsDeleted);
        Assert.Equal(_tuesday, removed.PublishedAtUtc);
        Assert.Equal("b", Assert.Single(removed.TextValues).Value);
    }

    [Fact]
    public void TheWorkingCopy_BecomesDraftsOfWhatChanged()
    {
        var built = Build(
            _legacy.Copy(1, _monday, "a", "b"),
            _legacy.Copy(2, null, "a", "B"));

        Assert.Single(built.Sheet.Versions);
        var drafts = Rows(built)
            .SelectMany(row => row.Revisions)
            .Where(revision => revision.Status == RevisionStatus.Draft)
            .ToList();
        var draft = Assert.Single(drafts);
        Assert.Equal("B", Assert.Single(draft.TextValues).Value);
        Assert.Null(draft.PublishedAtUtc);
        Assert.Null(draft.SheetVersion);
        Assert.Equal(_now, draft.CreatedAtUtc);
        Assert.Null(built.Checks[^1].MomentUtc);
    }

    [Fact]
    public void VersionsPublishedAtTheSameMoment_AreMovedApart()
    {
        var built = Build(
            _legacy.Copy(1, _monday, "a"),
            _legacy.Copy(2, _monday, "b"));

        var times = built.Sheet.Versions.Select(version => version.PublishedAtUtc).ToList();
        Assert.True(times[1] > times[0]);
        Assert.Single(built.Warnings);
    }

    [Fact]
    public void TheItems_KeepThePublicIdsOfTheirLatestCopy()
    {
        var first = _legacy.Copy(1, _monday, "a");
        var second = _legacy.Copy(2, _tuesday, "a");

        var built = Build(first, second);

        Assert.Equal(second.Snapshot.Rows[0].PublicId, Assert.Single(Rows(built)).PublicId);
        Assert.Equal(second.Snapshot.Tables[0].PublicId, Assert.Single(built.Sheet.Tables).PublicId);
    }

    [Fact]
    public void TwoWorkingCopies_CantBeMigrated()
    {
        Assert.Throws<MigrationException>(() => Build(
            _legacy.Copy(1, null, "a"),
            _legacy.Copy(2, null, "b")));
    }

    [Fact]
    public void AnUnpublishedCopyOlderThanAPublishedOne_CantBeMigrated()
    {
        Assert.Throws<MigrationException>(() => Build(
            _legacy.Copy(1, null, "a"),
            _legacy.Copy(2, _monday, "b")));
    }

    private static BuiltSheet Build(params LegacyCopy[] copies)
    {
        return SheetHistoryBuilder.Build(new DbSheet { Id = 7 }, copies, _now);
    }

    private static List<DbSheetRow> Rows(BuiltSheet built)
    {
        return built.Sheet.Tables.SelectMany(table => table.Sections).SelectMany(section => section.Rows).ToList();
    }
}
