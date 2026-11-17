using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Templates;

/// <summary>
/// Sets the name other applications look a cell up by, such as "partNumber". Null or blank removes it.
/// </summary>
public sealed record UpdateTemplateCellLookupKeyRequest
{
    [MaxLength(50)]
    public string? LookupKey { get; init; }
}
