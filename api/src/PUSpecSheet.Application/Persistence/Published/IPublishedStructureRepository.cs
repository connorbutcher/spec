using PUSpecSheet.Application.Published;

namespace PUSpecSheet.Application.Persistence.Published;

/// <summary>A sheet's tables, sections and column blocks as they were at a version.</summary>
public interface IPublishedStructureRepository
{
    Task<PublishedStructure> ReadAsync(ResolvedSheetVersion version, CancellationToken cancellationToken);
}
