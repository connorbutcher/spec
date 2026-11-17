namespace PUSpecSheet.Abstractions.Requests.Types.Properties;

public sealed record ManageSpecSheetTypePropertiesRequest(int SpecSheetTypeId,
    ICollection<CreateSpecSheetTypePropertyRequest> CreatePropertyRequests,
    ICollection<UpdateSpecSheetTypePropertyRequest> UpdatePropertyRequests,
    ICollection<int> DeletePropertyRequests);
