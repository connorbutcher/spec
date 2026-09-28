using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

public sealed record UpdateTemplateSectionRequest(
    [Required, MaxLength(100)] string Name,
    SectionInclusion Inclusion);
