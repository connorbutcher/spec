using PUSpecSheet.Application.Sheets.Removal;

namespace PUSpecSheet.Application.Tests.Sheets.Removal;

/// <summary>Sam (user 1) removes a section or table that may have other people's work inside it.</summary>
public sealed class HeldContentsTests
{
    private const int Sam = 1;
    private const int Alex = 2;
    private const int Kim = 3;
    private const int Lee = 4;

    private static readonly Dictionary<int, string> Names = new()
    {
        [Sam] = "Sam",
        [Alex] = "Alex",
        [Kim] = "Kim",
        [Lee] = "Lee",
    };

    [Fact]
    public void NothingHeldInside_CanBeRemoved()
    {
        Assert.Null(HeldContents.RefusalFor("this section", [], Sam, Names));
    }

    [Fact]
    public void MyOwnOpenDraftsInside_GoWithIt()
    {
        Assert.Null(HeldContents.RefusalFor("this section", [new ContainedHold(Sam, false), new ContainedHold(Sam, false)], Sam, Names));
    }

    [Fact]
    public void SomeoneElsesWorkInside_IsRefusedByName()
    {
        var refusal = HeldContents.RefusalFor("this section", [new ContainedHold(Sam, false), new ContainedHold(Alex, false)], Sam, Names);

        Assert.Equal("Alex is editing inside this section, so it can't be removed yet.", refusal);
    }

    [Fact]
    public void OnePersonHoldingSeveralThings_IsNamedOnce()
    {
        var refusal = HeldContents.RefusalFor("this table", [new ContainedHold(Alex, false), new ContainedHold(Alex, false)], Sam, Names);

        Assert.Equal("Alex is editing inside this table, so it can't be removed yet.", refusal);
    }

    [Fact]
    public void SeveralPeopleInside_AreAllNamed_Alphabetically()
    {
        Assert.Equal(
            "Alex and Kim are editing inside this table, so it can't be removed yet.",
            HeldContents.RefusalFor("this table", [new ContainedHold(Kim, false), new ContainedHold(Alex, false)], Sam, Names));
        Assert.Equal(
            "Alex, Kim and Lee are editing inside this table, so it can't be removed yet.",
            HeldContents.RefusalFor("this table", [new ContainedHold(Lee, false), new ContainedHold(Kim, false), new ContainedHold(Alex, false)], Sam, Names));
    }

    [Fact]
    public void MyOwnChangesWaitingForApproval_StopTheRemoval()
    {
        var refusal = HeldContents.RefusalFor("this section", [new ContainedHold(Sam, true)], Sam, Names);

        Assert.Equal("Changes inside this section are waiting for approval, so it can't be removed until they are approved or rejected.", refusal);
    }

    [Fact]
    public void ChangesWaitingForApproval_AreSaidBeforeWhoIsEditing()
    {
        var refusal = HeldContents.RefusalFor("this table", [new ContainedHold(Alex, false), new ContainedHold(Kim, true)], Sam, Names);

        Assert.Equal("Changes inside this table are waiting for approval, so it can't be removed until they are approved or rejected.", refusal);
    }

    [Fact]
    public void SomeoneWithNoName_IsAnotherUser()
    {
        var refusal = HeldContents.RefusalFor("this section", [new ContainedHold(99, false)], Sam, Names);

        Assert.Equal("Another user is editing inside this section, so it can't be removed yet.", refusal);
    }
}
