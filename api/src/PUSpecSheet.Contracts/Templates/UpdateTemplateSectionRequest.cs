using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

public sealed record UpdateTemplateSectionRequest([Required, MaxLength(100)] string Name);
