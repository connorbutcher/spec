using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>Adds a table template to the end of a sheet type's list.</summary>
public sealed record CreateTableTemplateRequest
{
    public CreateTableTemplateRequest(int sheetTypeId, [Required, MaxLength(100)] string name, TemplateOrientation orientation)
    {
        this.SheetTypeId = sheetTypeId;
        this.Name = name;
        this.Orientation = orientation;
    }

    public int SheetTypeId { get; init; }

    public string Name { get; init; }

    public TemplateOrientation Orientation { get; init; }
}
