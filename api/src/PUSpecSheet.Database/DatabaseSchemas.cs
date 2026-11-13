namespace PUSpecSheet.Database;

/// <summary>The SQL Server schemas the tables are grouped into. Nothing lives in dbo.</summary>
public static class DatabaseSchemas
{
    /// <summary>Sheets and everything on them: tables, sections, rows, column blocks, cells, their revisions and the published versions.</summary>
    public const string Spec = "spec";

    /// <summary>Table templates and their versions, sections, rows, cells and column blocks, and the cell types they use.</summary>
    public const string Template = "template";

    /// <summary>Drafts submitted for publishing and the approvers deciding them.</summary>
    public const string Approval = "approval";

    /// <summary>Users, roles and permissions.</summary>
    public const string Auth = "auth";

    /// <summary>The phases and sheet types sheets are organised by.</summary>
    public const string Setup = "setup";

    /// <summary>Typed tables holding the values entered in sheet cells, and the settings chosen for them on the sheet.</summary>
    public const string Cell = "cell";
}
