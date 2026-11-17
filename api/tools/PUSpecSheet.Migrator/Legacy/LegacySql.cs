namespace PUSpecSheet.Migrator.Legacy;

// The queries that read the legacy tables (api/legacy.txt). This is the one place that knows them, so if a name here is
// wrong, change it here and nothing else. Each query returns the columns LegacySheetReader and ReferenceDataMigrator
// read by name, using AS for the names they expect. Deleted tables, groups, rows and cells are left out: a copy
// doesn't have them.
internal static class LegacySql
{
    // The copies of one sheet: every SpecSheets row with the sheet's phase and sheet type.
    private const string SheetCopies = """
        SELECT s.[SpecSheetID]
        FROM [legacy].[SpecSheets] AS s
        WHERE s.[SpecSheetPhaseID] = @phaseId AND s.[SpecSheetTypeID] = @sheetTypeId
        """;

    private const string SheetTables = """
        SELECT t.[SpecSheetTableID]
        FROM [legacy].[SpecSheetTables] AS t
        WHERE t.[IsDeleted] = 0 AND t.[SpecSheetID] IN (
        """ + SheetCopies + ")";

    private const string SheetCells = """
        SELECT c.[SpecSheetTableCellID]
        FROM [legacy].[SpecSheetTableCells] AS c
        JOIN [legacy].[SpecSheetTableRows] AS r ON r.[SpecSheetTableRowID] = c.[SpecSheetTableRowID]
        JOIN [legacy].[SpecSheetTableGroups] AS g ON g.[SpecSheetTableGroupID] = r.[SpecSheetTableGroupID]
        WHERE c.[IsDeleted] = 0 AND g.[SpecSheetTableID] IN (
        """ + SheetTables + ")";

    public const string Sheets = """
        SELECT
            s.[SpecSheetPhaseID] AS [PhaseId],
            s.[SpecSheetTypeID] AS [SheetTypeId],
            MIN(s.[CreatedDateTime]) AS [CreatedAtUtc]
        FROM [legacy].[SpecSheets] AS s
        GROUP BY s.[SpecSheetPhaseID], s.[SpecSheetTypeID]
        ORDER BY s.[SpecSheetPhaseID], s.[SpecSheetTypeID];
        """;

    public const string Copies = """
        SELECT
            s.[SpecSheetID] AS [Id],
            s.[IsPublished],
            s.[PublishedDateTime] AS [PublishedAtUtc],
            s.[PublishedByUserID] AS [PublishedByUserId],
            s.[CreatedByUserID] AS [CreatedByUserId],
            s.[CreatedDateTime] AS [CreatedAtUtc],
            s.[ChangeLog],
            s.[LegacySpecSheetID] AS [LegacySpecSheetId],
            s.[HasBeenMigrated]
        FROM [legacy].[SpecSheets] AS s
        WHERE s.[SpecSheetPhaseID] = @phaseId AND s.[SpecSheetTypeID] = @sheetTypeId;
        """;

    public const string Tables = """
        SELECT
            t.[SpecSheetTableID] AS [Id],
            t.[SpecSheetID] AS [CopyId],
            t.[TableTemplateID] AS [TableTemplateId],
            t.[Title],
            t.[DisplayOrder],
            t.[ReferenceID] AS [ReferenceId],
            t.[ConfigurationOverride]
        FROM [legacy].[SpecSheetTables] AS t
        WHERE t.[SpecSheetTableID] IN (
        """ + SheetTables + ");";

    public const string Groups = """
        SELECT
            g.[SpecSheetTableGroupID] AS [Id],
            g.[SpecSheetTableID] AS [TableId],
            g.[TableGroupTemplateID] AS [GroupTemplateId],
            g.[Order],
            g.[ReferenceID] AS [ReferenceId],
            linked.[ReferenceID] AS [LinkedReferenceId]
        FROM [legacy].[SpecSheetTableGroups] AS g
        LEFT JOIN [legacy].[SpecSheetTableGroups] AS linked ON linked.[SpecSheetTableGroupID] = g.[LinkedSpecSheetTableGroupID]
        WHERE g.[IsDeleted] = 0 AND g.[SpecSheetTableID] IN (
        """ + SheetTables + ");";

    public const string Rows = """
        SELECT
            r.[SpecSheetTableRowID] AS [Id],
            r.[SpecSheetTableGroupID] AS [GroupId],
            r.[TableRowTemplateID] AS [RowTemplateId],
            r.[RowNumber],
            r.[ReferenceID] AS [ReferenceId]
        FROM [legacy].[SpecSheetTableRows] AS r
        JOIN [legacy].[SpecSheetTableGroups] AS g ON g.[SpecSheetTableGroupID] = r.[SpecSheetTableGroupID]
        WHERE r.[IsDeleted] = 0 AND g.[IsDeleted] = 0 AND g.[SpecSheetTableID] IN (
        """ + SheetTables + ");";

    // A legacy cell doesn't name its template cell: it is the one in its row's template at the same column.
    public const string Cells = """
        SELECT
            c.[SpecSheetTableCellID] AS [Id],
            c.[SpecSheetTableRowID] AS [RowId],
            c.[ColumnNumber],
            c.[ReferenceID] AS [ReferenceId],
            c.[TableCellTypeID] AS [CellTypeId],
            c.[CellConfigurationID] AS [CellConfigurationId],
            c.[ConfigurationOverride],
            ct.[TableCellTemplateID] AS [TemplateCellId],
            ct.[TableCellTypeID] AS [TemplateCellTypeId],
            ct.[CellConfigurationID] AS [TemplateCellConfigurationId]
        FROM [legacy].[SpecSheetTableCells] AS c
        JOIN [legacy].[SpecSheetTableRows] AS r ON r.[SpecSheetTableRowID] = c.[SpecSheetTableRowID]
        LEFT JOIN [template].[TableCellTemplates] AS ct
            ON ct.[TableRowTemplateID] = r.[TableRowTemplateID] AND ct.[ColumnNumber] = c.[ColumnNumber]
        WHERE c.[SpecSheetTableCellID] IN (
        """ + SheetCells + ");";

    public const string TextValues = """
        SELECT v.[TextValueID] AS [Id], v.[SpecSheetTableCellID] AS [CellId], v.[Value]
        FROM [legacy].[TextValues] AS v
        WHERE v.[Value] IS NOT NULL AND v.[SpecSheetTableCellID] IN (
        """ + SheetCells + ");";

    public const string NumericValues = """
        SELECT v.[NumericValueID] AS [Id], v.[SpecSheetTableCellID] AS [CellId], v.[Value]
        FROM [legacy].[NumericValues] AS v
        WHERE v.[Value] IS NOT NULL AND v.[SpecSheetTableCellID] IN (
        """ + SheetCells + ");";

    public const string PairValues = """
        SELECT v.[PairValueID] AS [Id], v.[SpecSheetTableCellID] AS [CellId], v.[PairValueX] AS [X], v.[PairValueY] AS [Y]
        FROM [legacy].[PairValues] AS v
        WHERE v.[SpecSheetTableCellID] IN (
        """ + SheetCells + ");";

    public const string LinkedValues = """
        SELECT
            v.[LinkedSpecSheetValueID] AS [Id],
            v.[SpecSheetTableCellID] AS [CellId],
            v.[LinkedSpecSheetTypeID] AS [SheetTypeId],
            v.[LinkedSpecSheetPhaseID] AS [PhaseId],
            v.[TableReferenceID] AS [TableReferenceId],
            v.[LinkedReferenceID] AS [ItemReferenceId]
        FROM [legacy].[LinkedSpecSheetValues] AS v
        WHERE v.[SpecSheetTableCellID] IN (
        """ + SheetCells + ");";

    public const string TableWidgets = """
        SELECT
            w.[SpecSheetTableWidgetID] AS [Id],
            w.[SpecSheetTableID] AS [OwnerId],
            w.[TableTemplateWidgetID] AS [TemplateWidgetId],
            v.[Value]
        FROM [legacy].[SpecSheetTableWidgets] AS w
        LEFT JOIN [legacy].[TableWidgetValues] AS v ON v.[SpecSheetTableWidgetID] = w.[SpecSheetTableWidgetID]
        WHERE w.[SpecSheetTableID] IN (
        """ + SheetTables + ")\nORDER BY w.[SpecSheetTableWidgetID], v.[TableWidgetValueID];";

    public const string GroupWidgets = """
        SELECT
            w.[SpecSheetTableGroupWidgetID] AS [Id],
            w.[SpecSheetTableGroupID] AS [OwnerId],
            w.[TableGroupTemplateWidgetID] AS [TemplateWidgetId],
            v.[Value]
        FROM [legacy].[SpecSheetTableGroupWidgets] AS w
        JOIN [legacy].[SpecSheetTableGroups] AS g ON g.[SpecSheetTableGroupID] = w.[SpecSheetTableGroupID]
        LEFT JOIN [legacy].[TableGroupWidgetValues] AS v ON v.[SpecSheetTableGroupWidgetID] = w.[SpecSheetTableGroupWidgetID]
        WHERE g.[SpecSheetTableID] IN (
        """ + SheetTables + ")\nORDER BY w.[SpecSheetTableGroupWidgetID], v.[TableGroupWidgetValueID];";

    public const string Notes = """
        SELECT
            n.[SpecSheetTableNoteID] AS [Id],
            n.[SpecSheetTableID] AS [TableId],
            n.[Note],
            n.[SourceSystem],
            n.[IsObsolete],
            n.[CreatedByUserID] AS [CreatedByUserId],
            n.[CreatedDateTime] AS [CreatedAtUtc]
        FROM [legacy].[SpecSheetTableNotes] AS n
        WHERE n.[SpecSheetTableID] IN (
        """ + SheetTables + ")\nORDER BY n.[CreatedDateTime], n.[SpecSheetTableNoteID];";

    public const string Properties = """
        SELECT
            v.[SpecSheetPropertyValueID] AS [Id],
            v.[SpecSheetID] AS [CopyId],
            v.[SpecSheetTypePropertyID] AS [PropertyId],
            v.[PropertyValue] AS [Value]
        FROM [legacy].[SpecSheetPropertyValues] AS v
        WHERE v.[SpecSheetID] IN (
        """ + SheetCopies + ")\nORDER BY v.[SpecSheetPropertyValueID];";

    public const string ExternalReferences = """
        SELECT
            e.[SpecSheetExternalReferenceID] AS [Id],
            e.[SpecSheetID] AS [CopyId],
            e.[ExternalID] AS [ExternalId],
            e.[ReferenceID] AS [ReferenceId],
            e.[SystemName],
            e.[OwnerName],
            e.[URL] AS [Url],
            e.[Type],
            e.[Grouping],
            e.[SystemIconURL] AS [SystemIconUrl],
            e.[Description]
        FROM [legacy].[SpecSheetExternalReferences] AS e
        WHERE e.[SpecSheetID] IN (
        """ + SheetCopies + ")\nORDER BY e.[SpecSheetExternalReferenceID];";

    public const string LinkedTables = """
        SELECT
            l.[SpecSheetLinkedTableID] AS [Id],
            l.[SpecSheetID] AS [CopyId],
            l.[TableReferenceID] AS [TableReferenceId],
            l.[DisplayOrder],
            l.[LinkedByUserID] AS [LinkedByUserId],
            l.[LinkedDateTime] AS [LinkedAtUtc]
        FROM [legacy].[SpecSheetLinkedTables] AS l
        WHERE l.[SpecSheetID] IN (
        """ + SheetCopies + ")\nORDER BY l.[SpecSheetLinkedTableID];";

    public const string WorkflowSteps = """
        SELECT
            s.[WorkflowStepID] AS [Id],
            s.[SpecSheetID] AS [CopyId],
            s.[StartedDateTime] AS [StartedAtUtc],
            s.[State]
        FROM [legacy].[WorkflowSteps] AS s
        WHERE s.[SpecSheetID] IN (
        """ + SheetCopies + ")\nORDER BY s.[StartedDateTime], s.[WorkflowStepID];";

    public const string WorkflowActors = """
        SELECT
            a.[WorkflowStepActorID] AS [Id],
            a.[WorkflowStepID] AS [StepId],
            a.[ActorUserID] AS [UserId],
            a.[Comment],
            a.[State],
            a.[ActionedDateTime] AS [ActionedAtUtc]
        FROM [legacy].[WorkflowStepActors] AS a
        JOIN [legacy].[WorkflowSteps] AS s ON s.[WorkflowStepID] = a.[WorkflowStepID]
        WHERE s.[SpecSheetID] IN (
        """ + SheetCopies + ")\nORDER BY a.[WorkflowStepActorID];";

    // The legacy design tables, read once.
    public const string GroupTemplates = """
        SELECT g.[TableGroupTemplateID] AS [Id], g.[TableTemplateID] AS [TableTemplateId]
        FROM [template].[TableGroupTemplates] AS g;
        """;

    public const string WidgetTypes = """
        SELECT w.[WidgetTypeID] AS [Id], w.[WidgetTypeKey] AS [Key], w.[Title] AS [Name], w.[Description]
        FROM [types].[WidgetTypes] AS w;
        """;

    public const string TableTemplateWidgets = """
        SELECT w.[TableTemplateWidgetID] AS [Id], w.[TableTemplateID] AS [TableTemplateId], w.[WidgetTypeID] AS [WidgetTypeId]
        FROM [template].[TableTemplateWidgets] AS w;
        """;

    public const string GroupTemplateWidgets = """
        SELECT w.[TableGroupTemplateWidgetID] AS [Id], w.[TableGroupTemplateID] AS [GroupTemplateId], w.[WidgetTypeID] AS [WidgetTypeId]
        FROM [template].[TableGroupTemplateWidgets] AS w;
        """;

    public const string SheetTypeProperties = """
        SELECT
            p.[SpecSheetTypePropertyID] AS [Id],
            p.[SpecSheetTypeID] AS [SheetTypeId],
            p.[ReferenceID] AS [PublicId],
            p.[ValueKey] AS [Key],
            p.[ValueName] AS [Name],
            p.[ValueType],
            p.[ValueControlType] AS [ControlType],
            p.[CanValueBeSetInForm] AS [CanBeSetInForm],
            p.[SaveValueAsProperty],
            p.[DisplayOrder],
            p.[PlaceholderText] AS [Placeholder],
            p.[ValidatorConfiguration]
        FROM [types].[SpecSheetTypeProperties] AS p;
        """;

    public const string UserIds = """
        SELECT u.[UserID] AS [Id], u.[DisplayName]
        FROM [access].[Users] AS u;
        """;

    // What each user could do in the legacy app, for the report: the new app's roles are given by hand.
    public const string UserAccess = """
        SELECT up.[UserID] AS [UserId], 'permission ' + p.[Key] AS [Access]
        FROM [access].[UserPermissions] AS up
        JOIN [access].[Permissions] AS p ON p.[PermissionID] = up.[PermissionID]
        UNION ALL
        SELECT tp.[UserID], 'table ' + CONVERT(nvarchar(36), tp.[SpecSheetTableReferenceID]) + CASE WHEN tp.[IsAdmin] = 1 THEN ' (admin)' ELSE '' END
        FROM [access].[SpecSheetTablePermissions] AS tp
        UNION ALL
        SELECT o.[UserID], 'observer of phase ' + CONVERT(nvarchar(10), o.[SpecSheetPhaseID]) + CASE WHEN o.[IsApprover] = 1 THEN ' (approver)' ELSE '' END
        FROM [access].[SpecSheetPhaseObservers] AS o
        ORDER BY [UserId], [Access];
        """;

    public const string SheetTypeAccess = """
        SELECT tp.[SpecSheetTypeID] AS [SheetTypeId], p.[Key] AS [Permission]
        FROM [access].[SpecSheetTypePermissions] AS tp
        JOIN [access].[Permissions] AS p ON p.[PermissionID] = tp.[PermissionID]
        ORDER BY tp.[SpecSheetTypeID], p.[Key];
        """;
}
