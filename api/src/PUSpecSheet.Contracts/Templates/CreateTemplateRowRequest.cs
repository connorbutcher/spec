using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Adds a row to a section, at 1-based <see cref="Position"/> or at the end when it's null. The new row
/// copies the columns of <see cref="CopyFromRowId"/>, or of the section's last row when that's null.
/// </summary>
public sealed record CreateTemplateRowRequest
{
    public CreateTemplateRowRequest(int templateSectionId, [Range(1, int.MaxValue)] int? position = null, int? copyFromRowId = null)
    {
        this.TemplateSectionId = templateSectionId;
        this.Position = position;
        this.CopyFromRowId = copyFromRowId;
    }

    public int TemplateSectionId { get; init; }

    public int? Position { get; init; }

    public int? CopyFromRowId { get; init; }
}
