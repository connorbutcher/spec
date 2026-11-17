using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.Application.SheetTypes;
using PUSpecSheet.Application.SheetTypes.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Seeding.ReferenceData;

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

    private static readonly SaveSheetTypeIdentifierRequest[] Definitions =
    [
        new(SerialNumber, SheetIdentifierKind.Text, true, null),
        new(BuildDate, SheetIdentifierKind.Date, false, null),
        new(Configuration, SheetIdentifierKind.Option, false, [BaseConfiguration, HighOutputConfiguration, HybridConfiguration]),
    ];

    private readonly PuSpecSheetDbContext db;
    private readonly ISheetTypesService sheetTypes;

    public ShowcaseSheetIdentifiers(PuSpecSheetDbContext db, ISheetTypesService sheetTypes)
    {
        this.db = db;
        this.sheetTypes = sheetTypes;
    }

    /// <summary>Gives the Engine Specifications sheet type its identifiers and returns their ids by name.</summary>
    public async Task<IReadOnlyDictionary<string, int>> EnsureAsync(CancellationToken cancellationToken)
    {
        var sheetTypeId = await SheetTypeQueries.GetIdByNameAsync(db, ShowcasePhases.EngineSpecificationsSheetType, cancellationToken);
        var sheetType = await SheetTypeQueries.FindAsync(db, sheetTypeId, cancellationToken)
            ?? throw new InvalidOperationException($"The {ShowcasePhases.EngineSpecificationsSheetType} sheet type was not found.");

        foreach (var definition in Definitions)
        {
            if (sheetType.Identifiers.All(identifier => !string.Equals(identifier.Name, definition.Name, StringComparison.OrdinalIgnoreCase)))
            {
                sheetType = await sheetTypes.AddIdentifierAsync(sheetTypeId, definition, cancellationToken);
            }
        }

        return sheetType.Identifiers.ToDictionary(identifier => identifier.Name, identifier => identifier.Id, StringComparer.OrdinalIgnoreCase);
    }
}
