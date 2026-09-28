namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Adds a row to the end of a section. The new row copies the columns of the section's last row,
/// so a table grows a row at a time.
/// </summary>
public sealed record CreateTemplateRowRequest(int TemplateSectionId);
