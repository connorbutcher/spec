using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public interface ITableTemplatesService
{
    /// <summary>Templates without their sections, optionally for one sheet type, in display order.</summary>
    Task<IReadOnlyList<TableTemplateSummary>> GetSummariesAsync(int? sheetTypeId, CancellationToken cancellationToken);

    /// <summary>A template at <paramref name="versionNumber"/>, or at its latest version when null.</summary>
    Task<TableTemplate> GetAsync(int id, int? versionNumber, CancellationToken cancellationToken);

    /// <summary>Adds a template, with an empty version 1, to the end of a sheet type's list.</summary>
    Task<TableTemplate> CreateAsync(CreateTableTemplateRequest request, CancellationToken cancellationToken);

    /// <summary>Renames the table. Changing orientation needs the latest version to be editable.</summary>
    Task<TableTemplate> UpdateAsync(int id, UpdateTableTemplateRequest request, CancellationToken cancellationToken);

    /// <summary>Copies the latest version into a new, editable version and returns it.</summary>
    Task<TableTemplate> CreateVersionAsync(int id, CancellationToken cancellationToken);

    /// <summary>Deletes a template no sheet uses, with all of its versions, sections, rows and cells.</summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
