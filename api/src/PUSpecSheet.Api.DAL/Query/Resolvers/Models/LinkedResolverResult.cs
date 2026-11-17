using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Api.DAL.Query.Resolvers.Models;

internal record LinkedResolverResult<T>
{
    public required SpecSheetType SpecSheetType { get; init; }

    public required T Result { get; init; }
}
