using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Migrator.Building;
using PUSpecSheet.Migrator.Legacy;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Tests.Building;

public sealed class SheetHistoryBuilderTests
{
    private static readonly DateTime _monday = new(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _tuesday = _monday.AddDays(1);
    private static readonly DateTime _wednesday = _monday.AddDays(2);
    private static readonly DateTime _now = _monday.AddDays(30);

    private readonly LegacySheetFactory _legacy = new();

    [Fact]
    public void EachPublishedCopy_BecomesAVersion_OldestFirst()
    {
        var built = Build(
            _legacy.Copy(_tuesday, "a"),
            _legacy.Copy(_monday, "a"));

        int[] numbers = [1, 2];
        DateTime[] times = [_monday, _tuesday];
        Assert.Equal(numbers, built.Sheet.Versions.Select(version => version.VersionNumber));
        Assert.Equal(times, built.Sheet.Versions.Select(version => version.PublishedAtUtc));
        Assert.All(built.Sheet.Versions, version => Assert.Equal(LegacySheetFactory.UserId, version.PublishedByUserId));
    }

    [Fact]
    public void AVersion_KeepsItsLegacyIdsAndChangeLog()
    {
        var copy = _legacy.Copy(_monday, "a") with { ChangeLog = "First issue", LegacySpecSheetId = 77, HasBeenMigrated = true };

        var version = Assert.Single(Build(copy).Sheet.Versions);

        Assert.Equal("First issue", version.Note);
        Assert.Equal(copy.Id, version.SourceSpecSheetId);
        Assert.Equal(77, version.LegacySpecSheetId);
        Assert.True(version.HasBeenMigrated);
    }

    [Fact]
    public void AnUnchangedRow_HasOneRevisionForEveryVersion()
    {
        var built = Build(
            _legacy.Copy(_monday, "a"),
            _legacy.Copy(_tuesday, "a"),
            _legacy.Copy(_wednesday, "a"));

        var row = Assert.Single(Rows(built));
        var revision = Assert.Single(row.Revisions);
        Assert.Equal(_monday, revision.PublishedAtUtc);
        Assert.Null(revision.SupersededAtUtc);
        Assert.Equal("a", Assert.Single(revision.TextValues).Value);
    }

    [Fact]
    public void ARowFollowedByItsReferenceId_GetsARevisionWhenItsTextChanges()
    {
        var built = Build(
            _legacy.Copy(_monday, "b"),
            _legacy.Copy(_tuesday, "B"));

        var row = Assert.Single(Rows(built));
        var revisions = row.Revisions.OrderBy(revision => revision.RevisionNumber).ToList();
        Assert.Equal(2, revisions.Count);
        Assert.Equal(_tuesday, revisions[0].SupersededAtUtc);
        Assert.Equal(_tuesday, revisions[1].PublishedAtUtc);
        Assert.Equal(2, revisions[1].SheetVersion!.VersionNumber);
        Assert.Equal("B", Assert.Single(revisions[1].TextValues).Value);
        Assert.Equal(_legacy.ReferenceOf("row b"), row.PublicId);
    }

    [Fact]
    public void ARowAddedInTheMiddle_LeavesTheRowsAroundItAlone()
    {
        var built = Build(
            _legacy.Copy(_monday, "a", "c"),
            _legacy.Copy(_tuesday, "a", "b", "c"));

        var rows = Rows(built);
        Assert.Equal(3, rows.Count);
        Assert.All(rows, row => Assert.Single(row.Revisions));
        var orders = rows.ToDictionary(row => row.Revisions.Single().TextValues.Single().Value, row => row.Revisions.Single().DisplayOrder);
        Assert.True(orders["a"] < orders["b"] && orders["b"] < orders["c"]);
    }

    [Fact]
    public void ARowMissingFromACopy_IsRemovedKeepingItsValues()
    {
        var built = Build(
            _legacy.Copy(_monday, "a", "b"),
            _legacy.Copy(_tuesday, "a"));

        var removed = Rows(built).SelectMany(row => row.Revisions).Single(revision => revision.IsDeleted);
        Assert.Equal(_tuesday, removed.PublishedAtUtc);
        Assert.Equal("b", Assert.Single(removed.TextValues).Value);
    }

    [Fact]
    public void TheWorkingCopy_BecomesDraftsOfWhatChanged()
    {
        var built = Build(
            _legacy.Copy(_monday, "a", "b"),
            _legacy.Copy(null, "a", "B"));

        Assert.Single(built.Sheet.Versions);
        var draft = Assert.Single(Rows(built).SelectMany(row => row.Revisions).Where(revision => revision.Status == RevisionStatus.Draft));
        Assert.Equal("B", Assert.Single(draft.TextValues).Value);
        Assert.Null(draft.SheetVersion);
        Assert.Equal(_now, draft.CreatedAtUtc);
        Assert.Null(built.Checks[^1].MomentUtc);
    }

    [Fact]
    public void ChangedWidgetValues_GiveTheTableARevision()
    {
        var built = Build(
            _legacy.Copy(_monday, snapshot => WithWidget(snapshot, "1"), "a"),
            _legacy.Copy(_tuesday, snapshot => WithWidget(snapshot, "2"), "a"));

        var table = Assert.Single(built.Sheet.Tables);
        Assert.Equal(2, table.Revisions.Count);
        Assert.Contains("\"2\"", table.Revisions.Single(revision => revision.SupersededAtUtc is null).WidgetValues, StringComparison.Ordinal);
    }

    [Fact]
    public void APropertyValue_GetsARevisionOnlyWhenItChanges()
    {
        var built = Build(
            _legacy.Copy(_monday, snapshot => snapshot with { Properties = new Dictionary<int, string?> { [3] = "x" } }, "a"),
            _legacy.Copy(_tuesday, snapshot => snapshot with { Properties = new Dictionary<int, string?> { [3] = "x" } }, "a"),
            _legacy.Copy(_wednesday, snapshot => snapshot with { Properties = new Dictionary<int, string?> { [3] = "y" } }, "a"));

        var property = Assert.Single(built.Sheet.Properties);
        Assert.Equal(3, property.SheetTypePropertyId);
        string?[] values = ["x", "y"];
        Assert.Equal(values, property.Revisions.OrderBy(revision => revision.RevisionNumber).Select(revision => revision.Value));
    }

    [Fact]
    public void AnExternalReferenceDroppedFromAVersion_GetsARemovingRevisionForThatVersion()
    {
        var reference = new SnapshotExternalReference { ReferenceId = Guid.NewGuid(), ExternalId = 1, SystemName = "ERP", OwnerName = "Ops", Url = "https://erp", Type = "Order" };
        var built = Build(
            _legacy.Copy(_monday, snapshot => snapshot with { ExternalReferences = [reference] }, "a"),
            _legacy.Copy(_tuesday, "a"));

        var revisions = Assert.Single(built.Sheet.ExternalReferences).Revisions.OrderBy(revision => revision.RevisionNumber).ToList();
        Assert.Equal(2, revisions.Count);
        Assert.True(revisions[1].IsDeleted);
        Assert.Equal(_tuesday, revisions[0].SupersededAtUtc);
        Assert.Equal(2, revisions[1].SheetVersion.VersionNumber);
    }

    [Fact]
    public void TheApprovingStep_IsTheSubmissionThatPublishedItsVersion()
    {
        var copy = _legacy.Copy(_monday, "a") with
        {
            Workflow =
            [
                new LegacyWorkflowStep { Id = 1, StartedAtUtc = _monday.AddHours(-2), State = 2, Actors = [new LegacyWorkflowActor { Id = 1, UserId = 8, State = 2, Comment = "No" }] },
                new LegacyWorkflowStep { Id = 2, StartedAtUtc = _monday.AddHours(-1), State = 1, Actors = [new LegacyWorkflowActor { Id = 2, UserId = 8, State = 1, ActionedAtUtc = _monday }] },
            ],
        };

        var built = Build(copy);

        Assert.Equal(2, built.Submissions.Count);
        var rejected = built.Submissions[0];
        Assert.Equal(SubmissionStatus.Rejected, rejected.Submission.Status);
        Assert.Null(rejected.Version);
        Assert.Empty(rejected.Revisions);
        Assert.Equal("No", Assert.Single(rejected.Submission.Approvers).Reason);

        var approved = built.Submissions[1];
        Assert.Equal(SubmissionStatus.Approved, approved.Submission.Status);
        Assert.Same(Assert.Single(built.Sheet.Versions), approved.Version);
        Assert.Equal(_monday, approved.Submission.DecidedAtUtc);
        Assert.NotEmpty(approved.Revisions);
    }

    [Fact]
    public void APendingStepOnTheWorkingCopy_HoldsItsDrafts()
    {
        var working = _legacy.Copy(null, "a", "b") with
        {
            Workflow = [new LegacyWorkflowStep { Id = 1, StartedAtUtc = _tuesday, State = 0, Actors = [new LegacyWorkflowActor { Id = 1, UserId = 8, State = 0 }] }],
        };

        var built = Build(_legacy.Copy(_monday, "a"), working);

        var pending = Assert.Single(built.Submissions);
        Assert.Equal(SubmissionStatus.Pending, pending.Submission.Status);
        Assert.Null(pending.Submission.DecidedAtUtc);
        Assert.All(pending.Revisions, revision => Assert.Equal(RevisionStatus.Draft, revision.Status));
        Assert.NotEmpty(pending.Revisions);
    }

    [Fact]
    public void AnUnknownWorkflowState_CantBeMigrated()
    {
        var copy = _legacy.Copy(_monday, "a") with { Workflow = [new LegacyWorkflowStep { Id = 1, StartedAtUtc = _monday, State = 9 }] };

        Assert.Throws<MigrationException>(() => Build(copy));
    }

    [Fact]
    public void VersionsPublishedAtTheSameMoment_AreMovedApart()
    {
        var built = Build(
            _legacy.Copy(_monday, "a"),
            _legacy.Copy(_monday, "b"));

        var times = built.Sheet.Versions.Select(version => version.PublishedAtUtc).ToList();
        Assert.True(times[1] > times[0]);
        Assert.Single(built.Warnings);
    }

    [Fact]
    public void TwoWorkingCopies_CantBeMigrated()
    {
        Assert.Throws<MigrationException>(() => Build(
            _legacy.Copy(null, "a"),
            _legacy.Copy(null, "b")));
    }

    private static SheetSnapshot WithWidget(SheetSnapshot snapshot, string value)
    {
        var widgets = SnapshotJson.WidgetValues(new Dictionary<int, string?> { [4] = value });
        return snapshot with { Tables = [snapshot.Tables[0] with { WidgetValues = widgets }] };
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
