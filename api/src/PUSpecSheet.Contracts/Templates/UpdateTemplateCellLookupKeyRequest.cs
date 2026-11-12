using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Sets the name other applications look a cell up by, such as "partNumber". Null or blank removes it.
/// </summary>
public sealed record UpdateTemplateCellLookupKeyRequest
{
    public UpdateTemplateCellLookupKeyRequest([MaxLength(50)] string? lookupKey)
    {
        this.LookupKey = lookupKey;
    }

    public string? LookupKey { get; init; }
}
