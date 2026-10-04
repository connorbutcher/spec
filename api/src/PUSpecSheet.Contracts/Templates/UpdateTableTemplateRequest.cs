using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

public sealed record UpdateTableTemplateRequest(
    [Required, MaxLength(100)] string Name,
    TemplateOrientation Orientation,
    [Range(0, 50)] int StickyColumnCount);
