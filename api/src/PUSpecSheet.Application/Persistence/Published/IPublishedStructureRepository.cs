using PUSpecSheet.Application.Published;
using PUSpecSheet.Application.Published.Versions;

namespace PUSpecSheet.Application.Persistence.Published;

/// <summary>A sheet's tables, sections and column blocks as they were at a version.</summary>
public interface IPublishedStructureRepository
{
    Task<PublishedStructure> ReadAsync(ResolvedSheetVersion version, CancellationToken cancellationToken);
}
