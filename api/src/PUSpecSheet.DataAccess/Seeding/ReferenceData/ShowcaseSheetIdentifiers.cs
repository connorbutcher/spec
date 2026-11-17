using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.DataAccess.SheetTypes;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Seeding.ReferenceData;

/// <summary>
/// The identifiers that tell the Engine Specifications sheets of a phase apart: a required serial number,
/// and an optional build date and configuration. They are added through the sheet types service, as the
/// admin page would, when the sheet type doesn't have one of that name already. Sheet types outlive a
/// rebuild of the showcase, so ones that exist are kept as they are.
/// </summary>
public sealed class ShowcaseSheetIdentifiers
{
    public const string SerialNumber = "Serial Number";
    public const string BuildDate = "Build Date";
    public const string Configuration = "Configuration";

    public const string BaseConfiguration = "Base";
    public const string HighOutputConfiguration = "High output";
    public const string HybridConfiguration = "Hybrid";

    private static readonly SaveSheetTypeIdentifierRequest[] _definitions =
    [
        new SaveSheetTypeIdentifierRequest { Name = SerialNumber, Kind = SheetIdentifierKind.Text, IsRequired = true },
        new SaveSheetTypeIdentifierRequest { Name = BuildDate, Kind = SheetIdentifierKind.Date, IsRequired = false },
        new SaveSheetTypeIdentifierRequest
        {
            Name = Configuration,
            Kind = SheetIdentifierKind.Option,
            IsRequired = false,
            Options = [BaseConfiguration, HighOutputConfiguration, HybridConfiguration],
        },
    ];

    private readonly PuSpecSheetDbContext _db;
    private readonly ISheetTypesService _sheetTypes;
    private readonly ISheetTypeDataService _sheetTypeData;

    public ShowcaseSheetIdentifiers(PuSpecSheetDbContext db, ISheetTypesService sheetTypes, ISheetTypeDataService sheetTypeData)
    {
        _db = db;
        _sheetTypes = sheetTypes;
        _sheetTypeData = sheetTypeData;
    }

    /// <summary>Gives the Engine Specifications sheet type its identifiers and returns their ids by name.</summary>
    public async Task<IReadOnlyDictionary<string, int>> EnsureAsync(CancellationToken cancellationToken)
    {
        var sheetTypeId = await _sheetTypeData.GetIdByNameAsync(ShowcasePhases.EngineSpecificationsSheetType, cancellationToken);
        var sheetType = await _sheetTypeData.FindAsync(sheetTypeId, cancellationToken)
            ?? throw new InvalidOperationException($"The {ShowcasePhases.EngineSpecificationsSheetType} sheet type was not found.");

        foreach (var definition in _definitions)
        {
            if (sheetType.Identifiers.All(identifier => !string.Equals(identifier.Name, definition.Name, StringComparison.OrdinalIgnoreCase)))
            {
                sheetType = await _sheetTypes.AddIdentifierAsync(sheetTypeId, definition, cancellationToken);
            }
        }

        return sheetType.Identifiers.ToDictionary(identifier => identifier.Name, identifier => identifier.Id, StringComparer.OrdinalIgnoreCase);
    }
}
