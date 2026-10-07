using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Tests.Sheets;

/// <summary>The counts the publish choice shows: my own changes, and everyone else's by person.</summary>
public sealed class DraftSummariesTests
{
    private const int Me = 1;
    private const int Asha = 2;
    private const int Ben = 3;

    private static readonly Dictionary<int, string> Names = new() { [Me] = "Me", [Asha] = "Asha", [Ben] = "Ben" };

    [Fact]
    public void MyCount_IsOnlyMyDrafts_OfEveryKind()
    {
        ISheetRevision[] drafts =
        [
            new SheetRowRevision { AuthorUserId = Me },
            new SheetTableRevision { AuthorUserId = Me },
            new SheetRowRevision { AuthorUserId = Asha },
        ];

        Assert.Equal(2, DraftSummaries.CountOf(drafts, Me));
    }

    [Fact]
    public void OtherPeoplesDrafts_AreCountedByPerson_InNameOrder_WithoutMine()
    {
        ISheetRevision[] drafts =
        [
            new SheetRowRevision { AuthorUserId = Ben },
            new SheetRowRevision { AuthorUserId = Me },
            new SheetSectionRevision { AuthorUserId = Asha },
            new SheetColumnBlockRevision { AuthorUserId = Asha },
        ];

        var others = DraftSummaries.OfOthers(drafts, Me, userId => Names[userId]);

        Assert.Equal([(Asha, "Asha", 2), (Ben, "Ben", 1)], others.Select(summary => (summary.UserId, summary.UserName, summary.DraftCount)));
    }

    [Fact]
    public void WithOnlyMyOwnDrafts_NobodyElseIsListed()
    {
        ISheetRevision[] drafts = [new SheetRowRevision { AuthorUserId = Me }];

        Assert.Empty(DraftSummaries.OfOthers(drafts, Me, userId => Names[userId]));
    }
}
