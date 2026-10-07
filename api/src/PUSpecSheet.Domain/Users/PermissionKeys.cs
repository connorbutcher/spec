namespace PUSpecSheet.Domain.Users;

/// <summary>
/// The key of every permission. Each one is also a row in the Permissions table and the name of the
/// API's authorization policy for it. To add one, add its key here, to <see cref="All"/> and to the
/// Permissions seed data, then create a migration.
/// </summary>
public static class PermissionKeys
{
    /// <summary>Add phases and choose which sheet types each phase has.</summary>
    public const string PhasesManage = "phases.manage";

    /// <summary>Add sheet types.</summary>
    public const string SheetTypesManage = "sheetTypes.manage";

    /// <summary>Create, change and delete table templates and everything in them.</summary>
    public const string TemplatesManage = "templates.manage";

    /// <summary>Create, change and delete cell types.</summary>
    public const string CellTypesManage = "cellTypes.manage";

    /// <summary>Change a sheet: its tables, sections, rows, column blocks and cell values. Changes stay drafts.</summary>
    public const string SheetsEdit = "sheets.edit";

    /// <summary>Publish a sheet's drafts as its next version.</summary>
    public const string SheetsPublish = "sheets.publish";

    public static IReadOnlyList<string> All { get; } =
    [
        PhasesManage,
        SheetTypesManage,
        TemplatesManage,
        CellTypesManage,
        SheetsEdit,
        SheetsPublish,
    ];
}
