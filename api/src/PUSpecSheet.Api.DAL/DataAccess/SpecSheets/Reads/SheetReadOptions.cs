namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

/// <summary>
/// The SheetReads configuration section.
/// </summary>
public class SheetReadOptions
{
    public const string SectionName = "SheetReads";

    /// <summary>
    /// Where published spec sheets are read from. Unpublished ones are always read from their copy.
    /// </summary>
    public SheetReadSource Source { get; set; } = SheetReadSource.Copies;
}
