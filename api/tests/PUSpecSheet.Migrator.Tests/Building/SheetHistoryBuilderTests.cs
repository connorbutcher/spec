using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Migrator.Building;
using PUSpecSheet.Migrator.Legacy;

namespace PUSpecSheet.Migrator.Tests.Building;

public sealed class SheetHistoryBuilderTests
{
    private static readonly DateTime Monday = new(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Tuesday = Monday.AddDays(1);
    private static readonly DateTime Wednesday = Monday.AddDays(2);
    private static readonly DateTime Now = Monday.AddDays(30);

    private readonly LegacySheetFactory legacy = new();

    [Fact]
    public void EachCopy_BecomesAVersion()
    {
        var built = Build(
            legacy.Copy(1, Monday, "a"),
            legacy.Copy(2, Tuesday, "a"));

        int[] numbers = [1, 2];
        DateTime[] times = [Monday, Tuesday];
        Assert.Equal(numbers, built.Sheet.Versions.Select(version => version.VersionNumber));
        Assert.Equal(times, built.Sheet.Versions.Select(version => version.PublishedAtUtc));
        Assert.All(built.Sheet.Versions, version => Assert.Equal(LegacySheetFactory.UserId, version.PublishedByUserId));
    }

    [Fact]
    public void AnUnchangedRow_HasOneRevisionForEveryVersion()
    {
        var built = Build(
            legacy.Copy(1, Monday, "a"),
            legacy.Copy(2, Tuesday, "a"),
            legacy.Copy(3, Wednesday, "a"));

        var row = Assert.Single(Rows(built));
        var revision = Assert.Single(row.Revisions);
        Assert.Equal(Monday, revision.PublishedAtUtc);
        Assert.Null(revision.SupersededAtUtc);
        Assert.Equal("a", Assert.Single(revision.TextValues).Value);
    }

    [Fact]
    public void AChangedRow_GetsARevisionThatReplacesTheLastAtTheSameMoment()
    {
        var built = Build(
            legacy.Copy(1, Monday, "a"),
            legacy.Copy(2, Tuesday, "b"));

        var row = Assert.Single(Rows(built));
        var revisions = row.Revisions.OrderBy(revision => revision.RevisionNumber).ToList();
        Assert.Equal(2, revisions.Count);
        Assert.Equal(Tuesday, revisions[0].SupersededAtUtc);
        Assert.Equal(Tuesday, revisions[1].PublishedAtUtc);
        Assert.Null(revisions[1].SupersededAtUtc);
        Assert.Equal(2, revisions[1].SheetVersion!.VersionNumber);
        Assert.Equal("b", Assert.Single(revisions[1].TextValues).Value);
    }

    [Fact]
    public void ARowAddedInTheMiddle_LeavesTheRowsAroundItAlone()
    {
        var built = Build(
            legacy.Copy(1, Monday, "a", "c"),
            legacy.Copy(2, Tuesday, "a", "b", "c"));

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
            legacy.Copy(1, Monday, "a", "b"),
            legacy.Copy(2, Tuesday, "a"));

        var removed = Rows(built)
            .SelectMany(row => row.Revisions)
            .Single(revision => revision.IsDeleted);
        Assert.Equal(Tuesday, removed.PublishedAtUtc);
        Assert.Equal("b", Assert.Single(removed.TextValues).Value);
    }

    [Fact]
    public void TheWorkingCopy_BecomesDraftsOfWhatChanged()
    {
        var built = Build(
            legacy.Copy(1, Monday, "a", "b"),
            legacy.Copy(2, null, "a", "B"));

        Assert.Single(built.Sheet.Versions);
        var drafts = Rows(built)
            .SelectMany(row => row.Revisions)
            .Where(revision => revision.Status == RevisionStatus.Draft)
            .ToList();
        var draft = Assert.Single(drafts);
        Assert.Equal("B", Assert.Single(draft.TextValues).Value);
        Assert.Null(draft.PublishedAtUtc);
        Assert.Null(draft.SheetVersion);
        Assert.Equal(Now, draft.CreatedAtUtc);
        Assert.Null(built.Checks[^1].MomentUtc);
    }

    [Fact]
    public void VersionsPublishedAtTheSameMoment_AreMovedApart()
    {
        var built = Build(
            legacy.Copy(1, Monday, "a"),
            legacy.Copy(2, Monday, "b"));

        var times = built.Sheet.Versions.Select(version => version.PublishedAtUtc).ToList();
        Assert.True(times[1] > times[0]);
        Assert.Single(built.Warnings);
    }

    [Fact]
    public void TheItems_KeepThePublicIdsOfTheirLatestCopy()
    {
        var first = legacy.Copy(1, Monday, "a");
        var second = legacy.Copy(2, Tuesday, "a");

        var built = Build(first, second);

        Assert.Equal(second.Snapshot.Rows[0].PublicId, Assert.Single(Rows(built)).PublicId);
        Assert.Equal(second.Snapshot.Tables[0].PublicId, Assert.Single(built.Sheet.Tables).PublicId);
    }

    [Fact]
    public void TwoWorkingCopies_CantBeMigrated()
    {
        Assert.Throws<MigrationException>(() => Build(
            legacy.Copy(1, null, "a"),
            legacy.Copy(2, null, "b")));
    }

    [Fact]
    public void AnUnpublishedCopyOlderThanAPublishedOne_CantBeMigrated()
    {
        Assert.Throws<MigrationException>(() => Build(
            legacy.Copy(1, null, "a"),
            legacy.Copy(2, Monday, "b")));
    }

    private static BuiltSheet Build(params LegacyCopy[] copies)
    {
        return SheetHistoryBuilder.Build(new DbSheet { Id = 7 }, copies, Now);
    }

    private static List<DbSheetRow> Rows(BuiltSheet built)
    {
        return built.Sheet.Tables.SelectMany(table => table.Sections).SelectMany(section => section.Rows).ToList();
    }
}
