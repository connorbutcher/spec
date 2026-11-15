namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// The queries that read the legacy layout. <b>This is the one place that knows its tables</b>, so if a name
/// here is wrong, change it and nothing else. Each query returns the columns <see cref="LegacySheetReader"/>
/// reads by name; the <c>AS</c> aliases are those names.
/// <para>
/// The legacy layout, in the <c>legacy</c> schema (see <c>database/OneTime/MoveFlatTablesToLegacy.sql</c>):
/// <c>PUSpecSheet</c>, one row per version of a sheet, the versions of one sheet sharing its phase and sheet
/// type; <c>Table</c>, the copy's tables; <c>TableGroup</c>, their groups (sections), nested by
/// <c>ParentGroupId</c>; <c>TableRow</c>, the groups' rows; <c>Cell</c>, the rows' cells; and value tables keyed by
/// <c>CellId</c>. Template ids are the ids of the design tables in the new database. Nothing in the legacy
/// layout is ordered, so items take the order of their templates, then of their ids.
/// </para>
/// </summary>
internal static class LegacySql
{
    public const string Sheets = """
        SELECT
            s.[PhaseId],
            s.[SheetTypeId],
            MIN(COALESCE(s.[PublishedAt], SYSUTCDATETIME())) AS [CreatedAtUtc],
            (
                SELECT TOP (1) latest.[PublicId]
                FROM [legacy].[PUSpecSheet] AS latest
                WHERE latest.[PhaseId] = s.[PhaseId] AND latest.[SheetTypeId] = s.[SheetTypeId]
                ORDER BY latest.[Version] DESC
            ) AS [PublicId]
        FROM [legacy].[PUSpecSheet] AS s
        GROUP BY s.[PhaseId], s.[SheetTypeId]
        ORDER BY s.[PhaseId], s.[SheetTypeId];
        """;

    /// <summary>A copy is published when its flag says so; otherwise it is the working copy, by its creator.</summary>
    public const string Versions = """
        SELECT
            s.[Id],
            s.[Version] AS [VersionNumber],
            CASE WHEN s.[Published] = 1 THEN s.[PublishedAt] END AS [PublishedAtUtc],
            CASE WHEN s.[Published] = 1 THEN s.[PublishedById] ELSE s.[CreatedById] END AS [UserId]
        FROM [legacy].[PUSpecSheet] AS s
        WHERE s.[PhaseId] = @phaseId AND s.[SheetTypeId] = @sheetTypeId;
        """;

    public const string Tables = """
        SELECT t.[Id], t.[SpecSheetId], t.[TableTemplateId], t.[Title]
        FROM [legacy].[Table] AS t
        JOIN [legacy].[PUSpecSheet] AS s ON s.[Id] = t.[SpecSheetId]
        WHERE s.[PhaseId] = @phaseId AND s.[SheetTypeId] = @sheetTypeId;
        """;

    public const string Groups = """
        SELECT g.[Id], t.[SpecSheetId], g.[TableId], g.[ParentGroupId], g.[GroupTemplateId]
        FROM [legacy].[TableGroup] AS g
        JOIN [legacy].[Table] AS t ON t.[Id] = g.[TableId]
        JOIN [legacy].[PUSpecSheet] AS s ON s.[Id] = t.[SpecSheetId]
        WHERE s.[PhaseId] = @phaseId AND s.[SheetTypeId] = @sheetTypeId;
        """;

    public const string Rows = """
        SELECT r.[Id], t.[SpecSheetId], r.[GroupId], r.[RowTemplateId]
        FROM [legacy].[TableRow] AS r
        JOIN [legacy].[TableGroup] AS g ON g.[Id] = r.[GroupId]
        JOIN [legacy].[Table] AS t ON t.[Id] = g.[TableId]
        JOIN [legacy].[PUSpecSheet] AS s ON s.[Id] = t.[SpecSheetId]
        WHERE s.[PhaseId] = @phaseId AND s.[SheetTypeId] = @sheetTypeId;
        """;

    public const string Cells = """
        SELECT c.[Id], t.[SpecSheetId], c.[TableRowId], c.[TemplateCellId]
        FROM [legacy].[Cell] AS c
        JOIN [legacy].[TableRow] AS r ON r.[Id] = c.[TableRowId]
        JOIN [legacy].[TableGroup] AS g ON g.[Id] = r.[GroupId]
        JOIN [legacy].[Table] AS t ON t.[Id] = g.[TableId]
        JOIN [legacy].[PUSpecSheet] AS s ON s.[Id] = t.[SpecSheetId]
        WHERE s.[PhaseId] = @phaseId AND s.[SheetTypeId] = @sheetTypeId;
        """;

    // The value queries all take the sheet's cells from this.
    private const string SheetCellIds = """
        SELECT c.[Id]
        FROM [legacy].[Cell] AS c
        JOIN [legacy].[TableRow] AS r ON r.[Id] = c.[TableRowId]
        JOIN [legacy].[TableGroup] AS g ON g.[Id] = r.[GroupId]
        JOIN [legacy].[Table] AS t ON t.[Id] = g.[TableId]
        JOIN [legacy].[PUSpecSheet] AS s ON s.[Id] = t.[SpecSheetId]
        WHERE s.[PhaseId] = @phaseId AND s.[SheetTypeId] = @sheetTypeId
        """;

    public const string TextValues =
        "SELECT [CellId], [Value] FROM [legacy].[TextValues] WHERE [CellId] IN (" + SheetCellIds + ");";

    public const string NumericValues =
        "SELECT [CellId], [Value] FROM [legacy].[NumericValues] WHERE [CellId] IN (" + SheetCellIds + ");";

    public const string DateValues =
        "SELECT [CellId], [Value] FROM [legacy].[DateValues] WHERE [CellId] IN (" + SheetCellIds + ");";

    public const string BooleanValues =
        "SELECT [CellId], [Value] FROM [legacy].[BooleanValues] WHERE [CellId] IN (" + SheetCellIds + ");";

    public const string OptionValues =
        "SELECT [CellId], [CellTypeOptionId] FROM [legacy].[OptionValues] WHERE [CellId] IN (" + SheetCellIds + ");";

    public const string LabelledTextValues =
        "SELECT [CellId], [SlotKey], [Position], [Value] FROM [legacy].[LabelledTextValues] WHERE [CellId] IN ("
        + SheetCellIds + ");";

    public const string CellSettings =
        "SELECT [CellId], [Settings] FROM [legacy].[CellSettings] WHERE [CellId] IN (" + SheetCellIds + ");";
}
