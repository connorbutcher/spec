using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Database.Models.CellTypes;

namespace PUSpecSheet.DataAccess.Seeding.ReferenceData;

// The cell types the showcase uses, once they exist: each one's id by name, and its kind and dropdown options by id,
// so templates can be laid out by name and sheet values written by what they are.
public sealed class ShowcaseCellTypeCatalog
{
    private readonly Dictionary<string, DbCellType> _byName;

    private readonly Dictionary<int, DbCellType> _byId;

    public ShowcaseCellTypeCatalog(IReadOnlyList<DbCellType> cellTypes)
    {
        _byName = cellTypes.ToDictionary(cellType => cellType.Name, StringComparer.OrdinalIgnoreCase);
        _byId = cellTypes.ToDictionary(cellType => cellType.Id);
    }

    public int this[string name] => _byName[name].Id;

    public CellKind KindOf(int cellTypeId)
    {
        return _byId[cellTypeId].Kind;
    }

    public int OptionId(int cellTypeId, string value)
    {
        var option = _byId[cellTypeId].Options.FirstOrDefault(candidate => string.Equals(candidate.Value, value, StringComparison.OrdinalIgnoreCase));
        return option?.Id ?? throw new InvalidOperationException($"'{_byId[cellTypeId].Name}' has no option '{value}'.");
    }
}
