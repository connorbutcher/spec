using PUSpecSheet.Domain.Phases;

namespace PUSpecSheet.Domain.SheetTypes;

/// <summary>A kind of PU Spec Sheet, such as Specification, Parts or Torque Sheet.</summary>
public class SheetType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public ICollection<PhaseSheetType> Phases { get; set; } = [];
}
