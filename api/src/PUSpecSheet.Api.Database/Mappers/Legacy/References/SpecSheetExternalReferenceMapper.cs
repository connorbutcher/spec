using PUSpecSheet.Abstractions.Domain.Legacy.References;
using PUSpecSheet.Api.Database.Schema.Legacy.References;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.References;

public static class SpecSheetExternalReferenceMapper
{
    public static SpecSheetExternalReference MapToDomain(this DbSpecSheetExternalReference dbSpecSheetExternalReference)
    {
        return new SpecSheetExternalReference
        {
            Id = dbSpecSheetExternalReference.Id,
            SpecSheetId = dbSpecSheetExternalReference.SpecSheetId,
            ExternalId = dbSpecSheetExternalReference.ExternalId,
            ReferenceId = dbSpecSheetExternalReference.ReferenceId,
            SystemName = dbSpecSheetExternalReference.SystemName,
            Grouping = dbSpecSheetExternalReference.Grouping,
            OwnerName = dbSpecSheetExternalReference.OwnerName,
            Type = dbSpecSheetExternalReference.Type,
            Url = dbSpecSheetExternalReference.Url,
            SystemIconUrl = dbSpecSheetExternalReference.SystemIconUrl,
            Description = dbSpecSheetExternalReference.Description,
        };
    }

    public static IEnumerable<SpecSheetExternalReference> MapToDomain(this ICollection<DbSpecSheetExternalReference> dbSpecSheetExternalReferences)
    {
        return dbSpecSheetExternalReferences.Select(MapToDomain);
    }

}
