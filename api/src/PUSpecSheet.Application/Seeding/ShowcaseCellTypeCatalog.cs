using PUSpecSheet.Database;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// The cell types the showcase uses, once they exist: each one's id by name, and its kind and dropdown
/// options by id, so templates can be laid out by name and sheet values written by what they are.
/// </summary>
public sealed class ShowcaseCellTypeCatalog
{
    private readonly Dictionary<string, DbCellType> byName;

    private readonly Dictionary<int, DbCellType> byId;

    public ShowcaseCellTypeCatalog(IReadOnlyList<DbCellType> cellTypes)
    {
        this.byName = cellTypes.ToDictionary(cellType => cellType.Name, StringComparer.OrdinalIgnoreCase);
        this.byId = cellTypes.ToDictionary(cellType => cellType.Id);
    }

    /// <summary>The id of the cell type called <paramref name="name"/>.</summary>
    public int this[string name] => byName[name].Id;

    /// <summary>The kind of the cell type <paramref name="cellTypeId"/>.</summary>
    public CellKind KindOf(int cellTypeId)
    {
        return byId[cellTypeId].Kind;
    }

    /// <summary>The id of the option of dropdown cell type <paramref name="cellTypeId"/> whose value is <paramref name="value"/>.</summary>
    public int OptionId(int cellTypeId, string value)
    {
        var option = byId[cellTypeId].Options.FirstOrDefault(candidate => string.Equals(candidate.Value, value, StringComparison.OrdinalIgnoreCase));
        return option?.Id ?? throw new InvalidOperationException($"'{byId[cellTypeId].Name}' has no option '{value}'.");
    }
}
