using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

public sealed record UpdateTableTemplateRequest
{
    public UpdateTableTemplateRequest(
        [Required, MaxLength(100)] string name,
        TemplateOrientation orientation,
        [Range(0, 50)] int stickyColumnCount)
    {
        this.Name = name;
        this.Orientation = orientation;
        this.StickyColumnCount = stickyColumnCount;
    }

    public string Name { get; init; }

    public TemplateOrientation Orientation { get; init; }

    public int StickyColumnCount { get; init; }
}
