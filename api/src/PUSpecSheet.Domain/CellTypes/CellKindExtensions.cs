namespace PUSpecSheet.Domain.CellTypes;

public static class CellKindExtensions
{
    /// <summary>Whether cells of this kind are filled in on a sheet, rather than only shown.</summary>
    public static bool StoresValue(this CellKind kind)
    {
        return kind is not (CellKind.Heading or CellKind.Group);
    }

    /// <summary>Whether cells of this kind keep their value as text in the text value table.</summary>
    public static bool StoresText(this CellKind kind)
    {
        return kind is CellKind.Text or CellKind.LinkedDropdown;
    }

    /// <summary>Whether cells of this kind pick one of the cell type's options.</summary>
    public static bool IsDropdown(this CellKind kind)
    {
        return kind is CellKind.TextDropdown or CellKind.NumberDropdown;
    }
}
