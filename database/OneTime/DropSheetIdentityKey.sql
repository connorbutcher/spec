/*
    ONE-TIME SCRIPT: removes spec.Sheets.IdentityKey from an existing PUSpecSheet database.

    A sheet's identity now lives only in spec.SheetIdentifierValues, and the API keeps it unique per phase and sheet
    type. The column held values (an empty string at least), so a deploy would stop rather than drop it. Run this once,
    by hand, before deploying the database project as usual (./database/deploy.ps1). The deploy then adds the plain
    (PhaseId, SheetTypeId) index.

    Nothing is lost: the column was the identifier values in one string, and those values are still in
    spec.SheetIdentifierValues. The script is safe to run again, and runs in one transaction, so a failure changes
    nothing.

    sqlcmd -S "(localdb)\MSSQLLocalDB" -d PUSpecSheet -b -i database/OneTime/DropSheetIdentityKey.sql
*/

SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRANSACTION;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Sheets_PhaseId_SheetTypeId_IdentityKey' AND object_id = OBJECT_ID(N'[spec].[Sheets]'))
BEGIN
    DROP INDEX [IX_Sheets_PhaseId_SheetTypeId_IdentityKey] ON [spec].[Sheets];
END;

IF OBJECT_ID(N'[spec].[DF_Sheets_IdentityKey]', N'D') IS NOT NULL
BEGIN
    ALTER TABLE [spec].[Sheets] DROP CONSTRAINT [DF_Sheets_IdentityKey];
END;

IF COL_LENGTH(N'spec.Sheets', N'IdentityKey') IS NOT NULL
BEGIN
    ALTER TABLE [spec].[Sheets] DROP COLUMN [IdentityKey];
END;

COMMIT TRANSACTION;

PRINT 'spec.Sheets.IdentityKey is gone. Now run ./database/deploy.ps1.';
