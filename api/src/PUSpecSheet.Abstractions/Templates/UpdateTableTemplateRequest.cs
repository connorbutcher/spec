using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Templates;

public sealed record UpdateTableTemplateRequest
{
    public UpdateTableTemplateRequest(
        [Required, MaxLength(100)] string name,
        TemplateOrientation orientation,
        [Range(0, 50)] int stickyColumnCount)
    {
        Name = name;
        Orientation = orientation;
        StickyColumnCount = stickyColumnCount;
    }

    public string Name { get; init; }

    public TemplateOrientation Orientation { get; init; }

    public int StickyColumnCount { get; init; }
}
