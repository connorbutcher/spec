using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>Adds a table template to the end of a sheet type's list.</summary>
public sealed record CreateTableTemplateRequest(
    int SheetTypeId,
    [Required, MaxLength(100)] string Name,
    TemplateOrientation Orientation);
