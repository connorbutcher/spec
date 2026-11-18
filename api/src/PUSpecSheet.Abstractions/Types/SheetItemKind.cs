namespace PUSpecSheet.Abstractions.Types;

/// <summary>
/// The items of a sheet that are checked out on their own: a table (its title, configuration, widgets and notes), a group
/// (its order and widgets) and a row (its order and its cells' values).
/// </summary>
public enum SheetItemKind
{
    Table,
    Group,
    Row
}
