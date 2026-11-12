namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// Adds a copy of an addable template section under a table (no parent) or under a section copy, which
/// must be a copy of the template section's own parent.
/// </summary>
public sealed record AddSheetSectionRequest
{
    public AddSheetSectionRequest(int templateSectionId, int? parentSheetSectionId)
    {
        this.TemplateSectionId = templateSectionId;
        this.ParentSheetSectionId = parentSheetSectionId;
    }

    public int TemplateSectionId { get; init; }

    public int? ParentSheetSectionId { get; init; }
}
