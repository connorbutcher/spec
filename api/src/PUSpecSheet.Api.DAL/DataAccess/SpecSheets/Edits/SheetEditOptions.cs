namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// The SheetEdits configuration section.
/// </summary>
public class SheetEditOptions
{
    public const string SectionName = "SheetEdits";

    /// <summary>
    /// The sheet types whose drafts are read from and edited on the revisions. Every other type is edited on its copies.
    /// </summary>
    public List<int> RevisionSheetTypeIds { get; set; } = [];
}