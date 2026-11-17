CREATE TABLE [spec].[SheetProperties] (
    [SheetPropertyID]         INT           IDENTITY (1, 1) NOT NULL,
    [SheetID]                 INT           NOT NULL,
    [SpecSheetTypePropertyID] INT           NOT NULL,
    [CreatedDateTime]         DATETIME2 (7) CONSTRAINT [DF_SheetProperties_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetProperties] PRIMARY KEY CLUSTERED ([SheetPropertyID] ASC),
    CONSTRAINT [FK_SheetProperties_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetProperties_SpecSheetTypeProperties_SpecSheetTypePropertyID] FOREIGN KEY ([SpecSheetTypePropertyID]) REFERENCES [types].[SpecSheetTypeProperties] ([SpecSheetTypePropertyID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetProperties_SheetID_SpecSheetTypePropertyID]
    ON [spec].[SheetProperties]([SheetID] ASC, [SpecSheetTypePropertyID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetProperties_SpecSheetTypePropertyID]
    ON [spec].[SheetProperties]([SpecSheetTypePropertyID] ASC);

