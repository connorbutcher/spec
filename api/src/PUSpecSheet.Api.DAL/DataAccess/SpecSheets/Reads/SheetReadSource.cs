namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

/// <summary>
/// Where a published spec sheet is read from while the revision tables are brought in.
/// </summary>
public enum SheetReadSource
{
    /// <summary>
    /// From its copy (SpecSheets and the tables under it), as before.
    /// </summary>
    Copies,

    /// <summary>
    /// From its copy, and from the revisions too, logging any difference between them.
    /// </summary>
    Compare,

    /// <summary>
    /// From the revisions (spec, cell and approval), or from its copy when it hasn't been migrated.
    /// </summary>
    Revisions
}
