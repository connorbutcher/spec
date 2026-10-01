using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Adds a column block to the right of a horizontal template version's other blocks. Each row of the
/// version gets one cell in the new block to start with.
/// </summary>
public sealed record CreateTemplateColumnBlockRequest(
    int TableTemplateVersionId,
    [Required, MaxLength(100)] string Name);
