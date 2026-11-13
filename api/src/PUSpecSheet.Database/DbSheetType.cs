namespace PUSpecSheet.Database;

/// <summary>A kind of PU Spec Sheet, such as Specification, Parts or Torque Sheet.</summary>
public class DbSheetType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public ICollection<DbPhaseSheetType> Phases { get; set; } = [];
}
