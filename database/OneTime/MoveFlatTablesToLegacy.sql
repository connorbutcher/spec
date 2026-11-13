/*
    ONE-TIME SCRIPT: sets aside the sheet tables of a database still on the flat layout, where every version of a
    sheet is a whole copy of it, so the revision layout can be deployed beside them and the migrator can move the
    sheets across.

    1. Run this once, by hand. It moves the flat sheet and value tables into a [legacy] schema with
       ALTER SCHEMA ... TRANSFER (rows, keys, indexes and constraints go with them; nothing is copied).
    2. Deploy the database project as usual (./database/deploy.ps1), which creates the new sheet tables.
    3. Run the migrator (api/tools/PUSpecSheet.Migrator), first as a dry run, then with --commit.
    4. Once the sheets are migrated and checked, drop the [legacy] schema's tables.

    The phases, sheet types, templates, cell types and users stay where they are: the migrated sheets point at
    them by id. The script refuses to run on a database that already has revision tables, as that is the new
    layout and its tables must not be moved. It is safe to run again, and runs in one transaction.

    sqlcmd -S "(localdb)\MSSQLLocalDB" -d PUSpecSheet -b -i database/OneTime/MoveFlatTablesToLegacy.sql
*/

SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'[spec].[SheetRowRevisions]', N'U') IS NOT NULL OR OBJECT_ID(N'[dbo].[SheetRowRevisions]', N'U') IS NOT NULL
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50000, N'This database already has revision tables, so it is on the new layout. Nothing was moved.', 1;
END

IF SCHEMA_ID(N'legacy') IS NULL EXEC (N'CREATE SCHEMA [legacy] AUTHORIZATION [dbo]');

DECLARE @moves TABLE ([Table] sysname NOT NULL PRIMARY KEY);

INSERT INTO @moves ([Table])
VALUES
    (N'Sheets'),
    (N'SheetVersions'),
    (N'SheetTables'),
    (N'SheetSections'),
    (N'SheetColumnBlocks'),
    (N'SheetRows'),
    (N'SheetCells'),
    (N'TextValues'),
    (N'NumericValues'),
    (N'DateValues'),
    (N'BooleanValues'),
    (N'OptionValues'),
    (N'LabelledTextValues'),
    (N'CellSettings');

DECLARE @table sysname, @from sysname, @sql nvarchar(max);

DECLARE moves CURSOR LOCAL FAST_FORWARD FOR
    SELECT [Table] FROM @moves;

OPEN moves;
FETCH NEXT FROM moves INTO @table;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(N'[legacy].' + QUOTENAME(@table), N'U') IS NOT NULL
    BEGIN
        PRINT N'Already in place: legacy.' + @table;
    END
    ELSE
    BEGIN
        SELECT @from = SCHEMA_NAME([schema_id])
        FROM sys.tables
        WHERE [name] = @table AND SCHEMA_NAME([schema_id]) IN (N'spec', N'cell', N'dbo', N'values');

        IF @from IS NULL
        BEGIN
            PRINT N'Not found: ' + @table;
        END
        ELSE
        BEGIN
            SET @sql = N'ALTER SCHEMA [legacy] TRANSFER ' + QUOTENAME(@from) + N'.' + QUOTENAME(@table) + N';';
            EXEC (@sql);
            PRINT N'Moved ' + @from + N'.' + @table + N' to legacy.' + @table;
        END
    END

    SET @from = NULL;
    FETCH NEXT FROM moves INTO @table;
END

CLOSE moves;
DEALLOCATE moves;

COMMIT TRANSACTION;
GO
