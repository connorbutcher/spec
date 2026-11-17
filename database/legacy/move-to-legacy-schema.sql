/*
    Moves the sheet copies' tables from dbo, values, references and workflow to the legacy schema, rows and all.

    deploy.ps1 does this by itself (the project's refactor log tells SqlPackage the tables moved), so this script is
    only for a database you would rather change by hand, or check, before deploying. It:
      - creates the legacy schema if it isn't there,
      - transfers each table that is still in its old schema (ALTER SCHEMA TRANSFER keeps rows, keys, indexes and
        identity values; nothing is copied),
      - records each move in dbo.__RefactorLog, as SqlPackage would, so a later deploy.ps1 doesn't try it again,
      - drops values, references and workflow once they are empty.

    It runs in one transaction and can be run again: a table already in legacy is skipped. It stops, changing
    nothing, if a table is in both its old schema and legacy (say, a deploy created an empty copy beside it); sort that
    out by hand first.

    Run it with: sqlcmd -S "(localdb)\MSSQLLocalDB" -d PUSpecSheet -E -b -i legacy\move-to-legacy-schema.sql
*/
SET XACT_ABORT ON;
SET NOCOUNT ON;

DECLARE @Moves TABLE
(
    [OldSchema]    SYSNAME          NOT NULL,
    [TableName]    SYSNAME          NOT NULL,
    [OperationKey] UNIQUEIDENTIFIER NOT NULL
);

-- The keys are the operations' keys in PUSpecSheet.Database.refactorlog.
INSERT INTO @Moves ([OldSchema], [TableName], [OperationKey])
VALUES
    (N'dbo', N'SpecSheetLinkedTables', '25f2e373-3041-41f5-a8a6-0b2cb6699146'),
    (N'dbo', N'SpecSheetPropertyValues', 'a82e9b5d-df38-402b-b4da-b5f832b80a06'),
    (N'dbo', N'SpecSheets', 'ad4158ba-a261-43c1-98fe-ce4a0ea12420'),
    (N'dbo', N'SpecSheetTableCells', 'b5313998-130e-4d39-a55f-65dc77be835f'),
    (N'dbo', N'SpecSheetTableGroups', 'adcadec9-85e4-4744-b1d3-a4804030dd71'),
    (N'dbo', N'SpecSheetTableGroupWidgets', 'db37a30a-debe-419f-b1fc-eba58127bd5b'),
    (N'dbo', N'SpecSheetTableNotes', '71ef54df-9c46-4acb-88b7-2f4099c76626'),
    (N'dbo', N'SpecSheetTableRows', '60881377-fbcf-40ca-bfd0-d02491709038'),
    (N'dbo', N'SpecSheetTables', '79319395-63ad-4b14-94f1-7e183192b9bc'),
    (N'dbo', N'SpecSheetTableWidgets', '010d40af-e92f-491d-a966-ee8320be60dc'),
    (N'values', N'LinkedSpecSheetValues', 'd3eb55ff-e465-4ea7-9c42-d6f537b55043'),
    (N'values', N'NumericValues', '52f21a90-88e4-432f-8814-d1c02c4df17c'),
    (N'values', N'PairValues', 'e0373034-d203-4236-9327-fb0383a5e5d3'),
    (N'values', N'TableGroupWidgetValues', '8eae7245-d20a-4254-9ab5-689b8de02f21'),
    (N'values', N'TableWidgetValues', 'bfcd3f17-3668-4180-9dfb-4fe53be2c1c8'),
    (N'values', N'TextValues', 'e85ff1db-c31b-4ca2-8424-cc566be675a2'),
    (N'references', N'SpecSheetExternalReferences', '164315bb-532d-4dcf-8aee-ef286bb18bee'),
    (N'workflow', N'WorkflowStepActors', '311fc2ce-1b0e-4837-b1af-65e88f681ca6'),
    (N'workflow', N'WorkflowSteps', 'e0ace85a-4afe-4074-b1ca-5c258d29b141');

IF EXISTS
(
    SELECT 1
    FROM @Moves AS [m]
    WHERE OBJECT_ID(QUOTENAME([m].[OldSchema]) + N'.' + QUOTENAME([m].[TableName]), N'U') IS NOT NULL
        AND OBJECT_ID(N'[legacy].' + QUOTENAME([m].[TableName]), N'U') IS NOT NULL
)
BEGIN
    SELECT [m].[OldSchema], [m].[TableName]
    FROM @Moves AS [m]
    WHERE OBJECT_ID(QUOTENAME([m].[OldSchema]) + N'.' + QUOTENAME([m].[TableName]), N'U') IS NOT NULL
        AND OBJECT_ID(N'[legacy].' + QUOTENAME([m].[TableName]), N'U') IS NOT NULL;

    THROW 50000, N'These tables are in both their old schema and legacy. Nothing was changed.', 1;
END;

BEGIN TRANSACTION;

IF SCHEMA_ID(N'legacy') IS NULL
BEGIN
    EXEC (N'CREATE SCHEMA [legacy] AUTHORIZATION [dbo];');
END;

IF OBJECT_ID(N'[dbo].[__RefactorLog]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[__RefactorLog]
    (
        [OperationKey] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
    );
END;

DECLARE @OldSchema SYSNAME;
DECLARE @TableName SYSNAME;
DECLARE @Sql NVARCHAR(MAX);

DECLARE [MoveCursor] CURSOR LOCAL FAST_FORWARD FOR
    SELECT [m].[OldSchema], [m].[TableName]
    FROM @Moves AS [m]
    WHERE OBJECT_ID(QUOTENAME([m].[OldSchema]) + N'.' + QUOTENAME([m].[TableName]), N'U') IS NOT NULL;

OPEN [MoveCursor];
FETCH NEXT FROM [MoveCursor] INTO @OldSchema, @TableName;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Sql = N'ALTER SCHEMA [legacy] TRANSFER ' + QUOTENAME(@OldSchema) + N'.' + QUOTENAME(@TableName) + N';';
    PRINT @Sql;
    EXEC (@Sql);
    FETCH NEXT FROM [MoveCursor] INTO @OldSchema, @TableName;
END;
CLOSE [MoveCursor];
DEALLOCATE [MoveCursor];

-- Every move whose table is now in legacy is done, whoever did it.
INSERT INTO [dbo].[__RefactorLog] ([OperationKey])
SELECT [m].[OperationKey]
FROM @Moves AS [m]
WHERE OBJECT_ID(N'[legacy].' + QUOTENAME([m].[TableName]), N'U') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM [dbo].[__RefactorLog] AS [r] WHERE [r].[OperationKey] = [m].[OperationKey]);

-- The old schemas held nothing else; drop them once they are empty.
DECLARE @Schema SYSNAME;
DECLARE [SchemaCursor] CURSOR LOCAL FAST_FORWARD FOR
    SELECT [s].[name]
    FROM [sys].[schemas] AS [s]
    WHERE [s].[name] IN (N'values', N'references', N'workflow')
        AND NOT EXISTS (SELECT 1 FROM [sys].[objects] AS [o] WHERE [o].[schema_id] = [s].[schema_id])
        AND NOT EXISTS (SELECT 1 FROM [sys].[types] AS [t] WHERE [t].[schema_id] = [s].[schema_id]);

OPEN [SchemaCursor];
FETCH NEXT FROM [SchemaCursor] INTO @Schema;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Sql = N'DROP SCHEMA ' + QUOTENAME(@Schema) + N';';
    PRINT @Sql;
    EXEC (@Sql);
    FETCH NEXT FROM [SchemaCursor] INTO @Schema;
END;
CLOSE [SchemaCursor];
DEALLOCATE [SchemaCursor];

COMMIT TRANSACTION;

PRINT N'The sheet copies'' tables are in the legacy schema.';
