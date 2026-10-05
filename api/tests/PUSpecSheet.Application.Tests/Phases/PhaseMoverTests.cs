using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Phases;
using PUSpecSheet.Domain.Phases;

namespace PUSpecSheet.Application.Tests.Phases;

/// <summary>
/// Moving a phase in this tree:
/// V6 (01-A2 (Rig), A3, A4), SC, TOP.
/// </summary>
public sealed class PhaseMoverTests
{
    private const int V6 = 1;
    private const int A2 = 2;
    private const int A3 = 3;
    private const int A4 = 4;
    private const int Sc = 5;
    private const int Top = 6;
    private const int Rig = 7;

    private readonly List<Phase> phases =
    [
        Phase(V6, "V6", null, 1),
        Phase(A2, "01-A2", V6, 1),
        Phase(A3, "A3", V6, 2),
        Phase(A4, "A4", V6, 3),
        Phase(Sc, "SC", null, 2),
        Phase(Top, "TOP", null, 3),
        Phase(Rig, "Rig", A2, 1),
    ];

    [Theory]
    [InlineData(A4, 1, "A4,01-A2,A3")]
    [InlineData(A4, 2, "01-A2,A4,A3")]
    [InlineData(A2, 3, "A3,A4,01-A2")]
    [InlineData(A2, 2, "A3,01-A2,A4")]
    [InlineData(A3, 2, "01-A2,A3,A4")]
    [InlineData(A2, 99, "A3,A4,01-A2")]
    public void MovingAmongSiblings_PutsThePhaseAtThePositionAndRenumbers(int id, int position, string expected)
    {
        PhaseMover.Move(phases, Find(id), V6, position);

        Assert.Equal(expected, CodesUnder(V6));
        Assert.Equal([1, 2, 3], OrdersUnder(V6));
    }

    [Fact]
    public void MovingUnderAnotherParent_JoinsItsPhasesAndClosesTheGapLeftBehind()
    {
        PhaseMover.Move(phases, Find(A3), A2, 1);

        Assert.Equal("A3,Rig", CodesUnder(A2));
        Assert.Equal([1, 2], OrdersUnder(A2));
        Assert.Equal("01-A2,A4", CodesUnder(V6));
        Assert.Equal([1, 2], OrdersUnder(V6));
    }

    [Fact]
    public void MovingToTheTopLevel_TakesThePhasesBeneathItAlong()
    {
        PhaseMover.Move(phases, Find(A2), null, 2);

        Assert.Equal("V6,01-A2,SC,TOP", CodesUnder(null));
        Assert.Equal([1, 2, 3, 4], OrdersUnder(null));
        Assert.Equal("Rig", CodesUnder(A2));
        Assert.Equal("A3,A4", CodesUnder(V6));
    }

    [Fact]
    public void MovingUnderAPhaseWithNoPhases_MakesItTheFirst()
    {
        PhaseMover.Move(phases, Find(Top), Sc, 5);

        Assert.Equal("TOP", CodesUnder(Sc));
        Assert.Equal([1], OrdersUnder(Sc));
        Assert.Equal("V6,SC", CodesUnder(null));
    }

    [Fact]
    public void SiblingsThatShareAnOrder_KeepTheirListedOrderWhenRenumbered()
    {
        Find(A3).DisplayOrder = 1;
        Find(A4).DisplayOrder = 1;

        PhaseMover.Move(phases, Find(Top), V6, 99);

        Assert.Equal("01-A2,A3,A4,TOP", CodesUnder(V6));
        Assert.Equal([1, 2, 3, 4], OrdersUnder(V6));
    }

    [Theory]
    [InlineData(V6, V6)]
    [InlineData(V6, A2)]
    [InlineData(V6, Rig)]
    [InlineData(A2, Rig)]
    public void MovingUnderItselfOrAPhaseBeneathIt_IsRefusedAndChangesNothing(int id, int parentId)
    {
        Assert.Throws<InvalidRequestException>(() => PhaseMover.Move(phases, Find(id), parentId, 1));

        Assert.Equal("V6,SC,TOP", CodesUnder(null));
        Assert.Equal("01-A2,A3,A4", CodesUnder(V6));
        Assert.Equal("Rig", CodesUnder(A2));
    }

    [Fact]
    public void MovingUnderAParentThatDoesNotExist_IsRefused()
    {
        Assert.Throws<NotFoundException>(() => PhaseMover.Move(phases, Find(A3), 99, 1));

        Assert.Equal(V6, Find(A3).ParentPhaseId);
    }

    [Fact]
    public void IsInSubtree_StopsOnDataThatLoopsBackOnItself()
    {
        Find(V6).ParentPhaseId = Rig;

        Assert.False(PhaseMover.IsInSubtree(phases, Sc, A3));
        Assert.True(PhaseMover.IsInSubtree(phases, A2, A3));
    }

    private static Phase Phase(int id, string code, int? parentId, int order)
    {
        return new Phase { Id = id, Code = code, ParentPhaseId = parentId, DisplayOrder = order };
    }

    private Phase Find(int id)
    {
        return phases.Single(phase => phase.Id == id);
    }

    private string CodesUnder(int? parentId)
    {
        return string.Join(',', Under(parentId).Select(phase => phase.Code));
    }

    private int[] OrdersUnder(int? parentId)
    {
        return Under(parentId).Select(phase => phase.DisplayOrder).ToArray();
    }

    private IEnumerable<Phase> Under(int? parentId)
    {
        return phases.Where(phase => phase.ParentPhaseId == parentId).OrderBy(phase => phase.DisplayOrder);
    }
}
