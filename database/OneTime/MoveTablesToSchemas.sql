/*
    ONE-TIME SCRIPT: moves an existing PUSpecSheet database onto the new schemas.

    Run it once, by hand, against a database created before the tables were grouped into schemas (everything in
    [dbo], the cell values in [values]), or one deployed while the design tables were in [template]. Then deploy the
    database project as usual (./database/deploy.ps1).

    ALTER SCHEMA ... TRANSFER moves each table with its rows, keys, indexes, constraints and foreign keys; nothing is
    copied or recreated. The script is safe to run again: a table already in its new schema is left alone, and it all
    runs in one transaction, so a failure changes nothing.

    sqlcmd -S "(localdb)\MSSQLLocalDB" -d PUSpecSheet -b -i database/OneTime/MoveTablesToSchemas.sql
*/

SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRANSACTION;

-- 1. The new schemas.
IF SCHEMA_ID(N'spec') IS NULL EXEC (N'CREATE SCHEMA [spec] AUTHORIZATION [dbo]');
IF SCHEMA_ID(N'design') IS NULL EXEC (N'CREATE SCHEMA [design] AUTHORIZATION [dbo]');
IF SCHEMA_ID(N'cell') IS NULL EXEC (N'CREATE SCHEMA [cell] AUTHORIZATION [dbo]');
IF SCHEMA_ID(N'approval') IS NULL EXEC (N'CREATE SCHEMA [approval] AUTHORIZATION [dbo]');
IF SCHEMA_ID(N'auth') IS NULL EXEC (N'CREATE SCHEMA [auth] AUTHORIZATION [dbo]');
IF SCHEMA_ID(N'setup') IS NULL EXEC (N'CREATE SCHEMA [setup] AUTHORIZATION [dbo]');

-- 2. Every table, from wherever it is now to its new schema.
DECLARE @moves TABLE ([Table] sysname NOT NULL PRIMARY KEY, [Schema] sysname NOT NULL);

INSERT INTO @moves ([Table], [Schema])
VALUES
    (N'Sheets', N'spec'),
    (N'SheetVersions', N'spec'),
    (N'SheetTables', N'spec'),
    (N'SheetTableRevisions', N'spec'),
    (N'SheetSections', N'spec'),
    (N'SheetSectionRevisions', N'spec'),
    (N'SheetColumnBlocks', N'spec'),
    (N'SheetColumnBlockRevisions', N'spec'),
    (N'SheetRows', N'spec'),
    (N'SheetRowRevisions', N'spec'),
    (N'SheetCells', N'spec'),
    (N'TableTemplates', N'design'),
    (N'TableTemplateVersions', N'design'),
    (N'TemplateSections', N'design'),
    (N'TemplateRows', N'design'),
    (N'TemplateCells', N'design'),
    (N'TemplateColumnBlocks', N'design'),
    (N'CellTypes', N'design'),
    (N'CellTypeOptions', N'design'),
    (N'SheetSubmissions', N'approval'),
    (N'SheetSubmissionApprovers', N'approval'),
    (N'Users', N'auth'),
    (N'Roles', N'auth'),
    (N'Permissions', N'auth'),
    (N'UserRoles', N'auth'),
    (N'RolePermissions', N'auth'),
    (N'Phases', N'setup'),
    (N'SheetTypes', N'setup'),
    (N'PhaseSheetTypes', N'setup'),
    (N'TextValues', N'cell'),
    (N'NumericValues', N'cell'),
    (N'DateValues', N'cell'),
    (N'BooleanValues', N'cell'),
    (N'OptionValues', N'cell'),
    (N'CellSettings', N'cell'),
    (N'LabelledTextValues', N'cell');

DECLARE @table sysname, @schema sysname, @from sysname, @sql nvarchar(max);

DECLARE moves CURSOR LOCAL FAST_FORWARD FOR
    SELECT [Table], [Schema] FROM @moves;

OPEN moves;
FETCH NEXT FROM moves INTO @table, @schema;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(QUOTENAME(@schema) + N'.' + QUOTENAME(@table), N'U') IS NOT NULL
    BEGIN
        PRINT N'Already in place: ' + @schema + N'.' + @table;
    END
    ELSE
    BEGIN
        SELECT @from = SCHEMA_NAME([schema_id])
        FROM sys.tables
        WHERE [name] = @table AND SCHEMA_NAME([schema_id]) IN (N'dbo', N'values', N'template');

        IF @from IS NULL
        BEGIN
            PRINT N'Not found, left for the deploy to create: ' + @table;
        END
        ELSE
        BEGIN
            SET @sql = N'ALTER SCHEMA ' + QUOTENAME(@schema) + N' TRANSFER ' + QUOTENAME(@from) + N'.' + QUOTENAME(@table) + N';';
            EXEC (@sql);
            PRINT N'Moved ' + @from + N'.' + @table + N' to ' + @schema + N'.' + @table;
        END
    END

    SET @from = NULL;
    FETCH NEXT FROM moves INTO @table, @schema;
END

CLOSE moves;
DEALLOCATE moves;

-- 3. The old schemas, once nothing is left in them.
IF SCHEMA_ID(N'values') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.objects WHERE [schema_id] = SCHEMA_ID(N'values'))
    EXEC (N'DROP SCHEMA [values]');

IF SCHEMA_ID(N'template') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.objects WHERE [schema_id] = SCHEMA_ID(N'template'))
    EXEC (N'DROP SCHEMA [template]');

COMMIT TRANSACTION;
GO
