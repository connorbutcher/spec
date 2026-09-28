using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Templates;

/// <summary>
/// Loads a whole template (sections, rows and cells) and returns it as the nested DTO. Every template
/// change returns this so the UI can replace its copy in one go.
/// </summary>
public sealed class TableTemplateReader(PuSpecSheetDbContext db)
{
    public async Task<TableTemplateDto> ReadAsync(int templateId, CancellationToken cancellationToken)
    {
        var template = await db.TableTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == templateId, cancellationToken);

        if (template is null)
        {
            throw new NotFoundException($"Table template {templateId} was not found.");
        }

        var sections = await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.TableTemplateId == templateId)
            .ToListAsync(cancellationToken);

        var rows = await db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => row.TemplateSection.TableTemplateId == templateId)
            .ToListAsync(cancellationToken);

        return template.ToDto(sections, rows);
    }
}
