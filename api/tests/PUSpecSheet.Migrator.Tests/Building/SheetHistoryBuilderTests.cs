using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Spec;
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
        Assert.Equal(times, built.Sheet.Versions.Select(version => version.PublishedDateTime));
        Assert.All(built.Sheet.Versions, version => Assert.Equal(LegacySheetFactory.UserId, version.PublishedByUserId));
    }

    [Fact]
    public void AVersion_KeepsItsLegacyIdsAndChangeLog()
    {
        var copy = _legacy.Copy(_monday, "a") with { ChangeLog = "First issue", LegacySpecSheetId = 77, HasBeenMigrated = true };

        var version = Assert.Single(Build(copy).Sheet.Versions);

        Assert.Equal("First issue", version.ChangeLog);
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
        Assert.Equal(_monday, revision.PublishedDateTime);
        Assert.Null(revision.SupersededDateTime);
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
        Assert.Equal(_tuesday, revisions[0].SupersededDateTime);
        Assert.Equal(_tuesday, revisions[1].PublishedDateTime);
        Assert.Equal(2, revisions[1].SheetVersion!.VersionNumber);
        Assert.Equal("B", Assert.Single(revisions[1].TextValues).Value);
        Assert.Equal(_legacy.ReferenceOf("row b"), row.ReferenceId);
    }

    [Fact]
    public void ARowAddedInTheMiddle_RenumbersTheRowsBelowItAsTheCopyDoes()
    {
        var built = Build(
            _legacy.Copy(_monday, "a", "c"),
            _legacy.Copy(_tuesday, "a", "b", "c"));

        var rows = Rows(built).ToDictionary(row => row.Revisions.First().TextValues.Single().Value);
        Assert.Equal(3, rows.Count);
        Assert.Equal(1, Assert.Single(rows["a"].Revisions).DisplayOrder);
        Assert.Equal(2, Assert.Single(rows["b"].Revisions).DisplayOrder);
        int[] numbers = [2, 3];
        Assert.Equal(numbers, rows["c"].Revisions.OrderBy(revision => revision.RevisionNumber).Select(revision => revision.DisplayOrder));
    }

    [Fact]
    public void AVersion_KeepsItsCopysCreatorAndABlankChangeLog()
    {
        var copy = _legacy.Copy(_monday, "a") with { ChangeLog = "", CreatedByUserId = 9, CreatedDateTime = _monday.AddHours(-3) };

        var version = Assert.Single(Build(copy).Sheet.Versions);

        Assert.Equal("", version.ChangeLog);
        Assert.Equal(9, version.SourceCreatedByUserId);
        Assert.Equal(_monday.AddHours(-3), version.SourceCreatedDateTime);
    }

    [Fact]
    public void ARowMissingFromACopy_IsRemovedKeepingItsValues()
    {
        var built = Build(
            _legacy.Copy(_monday, "a", "b"),
            _legacy.Copy(_tuesday, "a"));

        var removed = Rows(built).SelectMany(row => row.Revisions).Single(revision => revision.IsDeleted);
        Assert.Equal(_tuesday, removed.PublishedDateTime);
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
        Assert.Null(draft.PublishedDateTime);
        Assert.Equal(_now, draft.CreatedDateTime);
        Assert.Null(built.Checks[^1].Moment);
    }

    [Fact]
    public void ChangedWidgetValues_GiveTheTableARevision()
    {
        var built = Build(
            _legacy.Copy(_monday, snapshot => WithWidget(snapshot, "1"), "a"),
            _legacy.Copy(_tuesday, snapshot => WithWidget(snapshot, "2"), "a"));

        var table = Assert.Single(built.Sheet.Tables);
        Assert.Equal(2, table.Revisions.Count);
        var current = table.Revisions.Single(revision => revision.SupersededDateTime is null);
        var widget = Assert.IsType<PartPairWidgetValue>(Assert.Single(current.WidgetValues!).Value);
        Assert.Equal("2", widget.TopValue);
    }

    [Fact]
    public void ACellsConfigurationOverride_IsKeptWithItsValue()
    {
        var properties = new TextCellProperties();
        var built = Build(_legacy.Copy(
            _monday,
            snapshot => snapshot with { Cells = [snapshot.Cells[0] with { Content = snapshot.Cells[0].Content with { ConfigurationOverride = properties } }] },
            "a"));

        var revision = Assert.Single(Assert.Single(Rows(built)).Revisions);
        Assert.Same(properties, Assert.Single(revision.CellConfigurationOverrides).ConfigurationOverride);
        Assert.Equal("a", Assert.Single(revision.TextValues).Value);
    }

    [Fact]
    public void APropertyValue_GetsARevisionOnlyWhenItChanges()
    {
        var built = Build(
            _legacy.Copy(_monday, snapshot => snapshot with { Properties = new Dictionary<int, string?> { [3] = "x" } }, "a"),
            _legacy.Copy(_tuesday, snapshot => snapshot with { Properties = new Dictionary<int, string?> { [3] = "x" } }, "a"),
            _legacy.Copy(_wednesday, snapshot => snapshot with { Properties = new Dictionary<int, string?> { [3] = "y" } }, "a"));

        var property = Assert.Single(built.Sheet.Properties);
        Assert.Equal(3, property.SpecSheetTypePropertyId);
        string?[] values = ["x", "y"];
        Assert.Equal(values, property.Revisions.OrderBy(revision => revision.RevisionNumber).Select(revision => revision.PropertyValue));
    }

    [Fact]
    public void AnExternalReferenceDroppedFromAVersion_GetsARemovingRevisionForThatVersion()
    {
        var reference = new SnapshotExternalReference
        {
            ReferenceId = Guid.NewGuid(),
            ExternalId = 1,
            SystemName = "ERP",
            OwnerName = "Ops",
            Url = "https://erp",
            Type = "Order",
            Grouping = null,
            SystemIconUrl = null,
            Description = null
        };
        var built = Build(
            _legacy.Copy(_monday, snapshot => snapshot with { ExternalReferences = [reference] }, "a"),
            _legacy.Copy(_tuesday, "a"));

        var revisions = Assert.Single(built.Sheet.ExternalReferences).Revisions.OrderBy(revision => revision.RevisionNumber).ToList();
        Assert.Equal(2, revisions.Count);
        Assert.True(revisions[1].IsDeleted);
        Assert.Equal(_tuesday, revisions[0].SupersededDateTime);
        Assert.Equal(2, revisions[1].SheetVersion!.VersionNumber);
    }

    [Fact]
    public void TheApprovingStep_IsTheSubmissionThatPublishedItsVersion()
    {
        var copy = _legacy.Copy(_monday, "a") with
        {
            Workflow =
            [
                Step(1, _monday.AddHours(-2), WorkflowState.Rejected, Actor(1, 8, WorkflowState.Rejected, null, "No")),
                Step(2, _monday.AddHours(-1), WorkflowState.Approved, Actor(2, 8, WorkflowState.Approved, _monday, null))
            ]
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
        Assert.Equal(_monday, approved.Submission.DecidedDateTime);
        Assert.NotEmpty(approved.Revisions);
    }

    [Fact]
    public void APendingStepOnTheWorkingCopy_HoldsItsDrafts()
    {
        var working = _legacy.Copy(null, "a", "b") with
        {
            Workflow = [Step(1, _tuesday, WorkflowState.Pending, Actor(1, 8, WorkflowState.Pending, null, null))]
        };

        var built = Build(_legacy.Copy(_monday, "a"), working);

        var pending = Assert.Single(built.Submissions);
        Assert.Equal(SubmissionStatus.Pending, pending.Submission.Status);
        Assert.Null(pending.Submission.DecidedDateTime);
        Assert.All(pending.Revisions, revision => Assert.Equal(RevisionStatus.Draft, revision.Status));
        Assert.NotEmpty(pending.Revisions);
    }

    [Fact]
    public void ARejectionWithoutAComment_IsGivenAReason()
    {
        var copy = _legacy.Copy(_monday, "a") with
        {
            Workflow = [Step(1, _monday.AddHours(-1), WorkflowState.Rejected, Actor(1, 8, WorkflowState.Rejected, _monday, " "))]
        };

        var approver = Assert.Single(Assert.Single(Build(copy).Submissions).Submission.Approvers);

        Assert.Equal(ApproverDecision.Rejected, approver.Decision);
        Assert.False(string.IsNullOrWhiteSpace(approver.Reason));
    }

    [Fact]
    public void AnUnknownWorkflowState_CantBeMigrated()
    {
        var copy = _legacy.Copy(_monday, "a") with
        {
            Workflow = [new LegacyWorkflowStep { Id = 1, StartedDateTime = _monday, State = 9, Actors = [] }]
        };

        Assert.Throws<MigrationException>(() => Build(copy));
    }

    [Fact]
    public void VersionsPublishedAtTheSameMoment_AreMovedApart()
    {
        var built = Build(
            _legacy.Copy(_monday, "a"),
            _legacy.Copy(_monday, "b"));

        var times = built.Sheet.Versions.Select(version => version.PublishedDateTime).ToList();
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

    [Fact]
    public void ItemsKeepTheirReferenceIds()
    {
        var built = Build(_legacy.Copy(_monday, "a"));

        Assert.Equal(_legacy.ReferenceOf("table"), Assert.Single(built.Sheet.Tables).ReferenceId);
        Assert.Equal(_legacy.ReferenceOf("group"), Assert.Single(Assert.Single(built.Sheet.Tables).Groups).ReferenceId);
        Assert.Equal(_legacy.ReferenceOf("cell a"), Assert.Single(Assert.Single(Rows(built)).Cells).ReferenceId);
    }

    private static SheetSnapshot WithWidget(SheetSnapshot snapshot, string value)
    {
        SheetWidgetValue[] widgets = [new SheetWidgetValue { TemplateWidgetId = 4, Value = new PartPairWidgetValue { TopValue = value, BottomValue = "-" } }];
        return snapshot with { Tables = [snapshot.Tables[0] with { WidgetValues = widgets }] };
    }

    private static LegacyWorkflowStep Step(int id, DateTime started, WorkflowState state, params LegacyWorkflowActor[] actors)
    {
        return new LegacyWorkflowStep { Id = id, StartedDateTime = started, State = (int)state, Actors = actors };
    }

    private static LegacyWorkflowActor Actor(int id, int userId, WorkflowState state, DateTime? actioned, string? comment)
    {
        return new LegacyWorkflowActor { Id = id, UserId = userId, State = (int)state, ActionedDateTime = actioned, Comment = comment };
    }

    private static BuiltSheet Build(params LegacyCopy[] copies)
    {
        var sheet = new DbSheet
        {
            Id = 7,
            SpecSheetPhaseId = 1,
            SpecSheetTypeId = 2,
            ReferenceId = Guid.NewGuid(),
            CreatedDateTime = _monday,
            LegacySourceHash = null,
            MigratedDateTime = null
        };
        return SheetHistoryBuilder.Build(sheet, copies, _now);
    }

    private static List<DbSheetTableRow> Rows(BuiltSheet built)
    {
        return built.Sheet.Tables.SelectMany(table => table.Groups).SelectMany(group => group.Rows).ToList();
    }
}
