namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// The queries that read the legacy layout. <b>This is the one place that knows its shape</b>, so if the legacy
/// tables differ from what is assumed here, change these and nothing else. Each query returns the columns
/// <see cref="LegacySheetReader"/> reads by name.
/// <para>
/// Assumed shape, in the <c>legacy</c> schema: <c>Sheets</c> (one per phase and sheet type);
/// <c>SheetVersions</c> (one per copy, <c>PublishedAtUtc</c> null for the working copy); and per copy the same
/// item tables the current schema has, <c>SheetTables</c> (with <c>SheetVersionId</c>), <c>SheetSections</c>,
/// <c>SheetColumnBlocks</c>, <c>SheetRows</c> and <c>SheetCells</c>, carrying the title and display order that
/// now live on the revisions. The value tables mirror the <c>cell</c> schema's but are keyed by
/// <c>SheetCellId</c> alone, as each copy has its own cells.
/// </para>
/// </summary>
internal static class LegacySql
{
    public const string Sheets = """
        SELECT [Id], [PublicId], [PhaseId], [SheetTypeId], [CreatedAtUtc]
        FROM [legacy].[Sheets]
        ORDER BY [Id];
        """;

    public const string Versions = """
        SELECT [Id], [VersionNumber], [PublishedAtUtc], [PublishedByUserId] AS [UserId], [Note]
        FROM [legacy].[SheetVersions]
        WHERE [SheetId] = @sheetId;
        """;

    public const string Tables = """
        SELECT t.[Id], t.[PublicId], t.[SheetVersionId], t.[TableTemplateVersionId], t.[Title], t.[DisplayOrder]
        FROM [legacy].[SheetTables] AS t
        JOIN [legacy].[SheetVersions] AS v ON v.[Id] = t.[SheetVersionId]
        WHERE v.[SheetId] = @sheetId;
        """;

    public const string Sections = """
        SELECT s.[Id], s.[PublicId], t.[SheetVersionId], s.[SheetTableId], s.[ParentSheetSectionId],
            s.[TemplateSectionId], s.[DisplayOrder]
        FROM [legacy].[SheetSections] AS s
        JOIN [legacy].[SheetTables] AS t ON t.[Id] = s.[SheetTableId]
        JOIN [legacy].[SheetVersions] AS v ON v.[Id] = t.[SheetVersionId]
        WHERE v.[SheetId] = @sheetId;
        """;

    public const string ColumnBlocks = """
        SELECT b.[Id], b.[PublicId], t.[SheetVersionId], b.[SheetTableId], b.[TemplateColumnBlockId], b.[DisplayOrder]
        FROM [legacy].[SheetColumnBlocks] AS b
        JOIN [legacy].[SheetTables] AS t ON t.[Id] = b.[SheetTableId]
        JOIN [legacy].[SheetVersions] AS v ON v.[Id] = t.[SheetVersionId]
        WHERE v.[SheetId] = @sheetId;
        """;

    public const string Rows = """
        SELECT r.[Id], r.[PublicId], t.[SheetVersionId], r.[SheetSectionId], r.[TemplateRowId], r.[DisplayOrder]
        FROM [legacy].[SheetRows] AS r
        JOIN [legacy].[SheetSections] AS s ON s.[Id] = r.[SheetSectionId]
        JOIN [legacy].[SheetTables] AS t ON t.[Id] = s.[SheetTableId]
        JOIN [legacy].[SheetVersions] AS v ON v.[Id] = t.[SheetVersionId]
        WHERE v.[SheetId] = @sheetId;
        """;

    public const string Cells = """
        SELECT c.[Id], c.[PublicId], t.[SheetVersionId], c.[SheetRowId], c.[TemplateCellId], c.[SheetColumnBlockId]
        FROM [legacy].[SheetCells] AS c
        JOIN [legacy].[SheetRows] AS r ON r.[Id] = c.[SheetRowId]
        JOIN [legacy].[SheetSections] AS s ON s.[Id] = r.[SheetSectionId]
        JOIN [legacy].[SheetTables] AS t ON t.[Id] = s.[SheetTableId]
        JOIN [legacy].[SheetVersions] AS v ON v.[Id] = t.[SheetVersionId]
        WHERE v.[SheetId] = @sheetId;
        """;

    // The value queries all take the sheet's cells from this.
    private const string SheetCellIds = """
        SELECT c.[Id]
        FROM [legacy].[SheetCells] AS c
        JOIN [legacy].[SheetRows] AS r ON r.[Id] = c.[SheetRowId]
        JOIN [legacy].[SheetSections] AS s ON s.[Id] = r.[SheetSectionId]
        JOIN [legacy].[SheetTables] AS t ON t.[Id] = s.[SheetTableId]
        JOIN [legacy].[SheetVersions] AS v ON v.[Id] = t.[SheetVersionId]
        WHERE v.[SheetId] = @sheetId
        """;

    public const string TextValues =
        "SELECT [SheetCellId], [Value] FROM [legacy].[TextValues] WHERE [SheetCellId] IN (" + SheetCellIds + ");";

    public const string NumericValues =
        "SELECT [SheetCellId], [Value] FROM [legacy].[NumericValues] WHERE [SheetCellId] IN (" + SheetCellIds + ");";

    public const string DateValues =
        "SELECT [SheetCellId], [Value] FROM [legacy].[DateValues] WHERE [SheetCellId] IN (" + SheetCellIds + ");";

    public const string BooleanValues =
        "SELECT [SheetCellId], [Value] FROM [legacy].[BooleanValues] WHERE [SheetCellId] IN (" + SheetCellIds + ");";

    public const string OptionValues =
        "SELECT [SheetCellId], [CellTypeOptionId] FROM [legacy].[OptionValues] WHERE [SheetCellId] IN (" + SheetCellIds + ");";

    public const string LabelledTextValues =
        "SELECT [SheetCellId], [SlotKey], [Position], [Value] FROM [legacy].[LabelledTextValues] WHERE [SheetCellId] IN ("
        + SheetCellIds + ");";

    public const string CellSettings =
        "SELECT [SheetCellId], [Settings] FROM [legacy].[CellSettings] WHERE [SheetCellId] IN (" + SheetCellIds + ");";
}
