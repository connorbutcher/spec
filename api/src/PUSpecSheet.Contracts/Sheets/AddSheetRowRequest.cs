namespace PUSpecSheet.Contracts.Sheets;

/// <summary>Adds another row built from one of the template rows of the section's template section.</summary>
public sealed record AddSheetRowRequest(int TemplateRowId);
