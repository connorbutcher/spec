CREATE TABLE [spec].[Sheets] (
    [SheetID]          INT              IDENTITY (1, 1) NOT NULL,
    [SpecSheetPhaseID] SMALLINT         NOT NULL,
    [SpecSheetTypeID]  TINYINT          NOT NULL,
    [ReferenceID]      UNIQUEIDENTIFIER CONSTRAINT [DF_Sheets_ReferenceID] DEFAULT (newsequentialid()) NOT NULL,
    [CreatedDateTime]  DATETIME2 (7)    CONSTRAINT [DF_Sheets_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [RowVersion]       ROWVERSION       NOT NULL,
    CONSTRAINT [PK_Sheets] PRIMARY KEY CLUSTERED ([SheetID] ASC),
    CONSTRAINT [FK_Sheets_SpecSheetPhases_SpecSheetPhaseID] FOREIGN KEY ([SpecSheetPhaseID]) REFERENCES [types].[SpecSheetPhases] ([SpecSheetPhaseID]),
    CONSTRAINT [FK_Sheets_SpecSheetTypes_SpecSheetTypeID] FOREIGN KEY ([SpecSheetTypeID]) REFERENCES [types].[SpecSheetTypes] ([SpecSheetTypeID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Sheets_SpecSheetPhaseID_SpecSheetTypeID]
    ON [spec].[Sheets]([SpecSheetPhaseID] ASC, [SpecSheetTypeID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Sheets_SpecSheetTypeID]
    ON [spec].[Sheets]([SpecSheetTypeID] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Sheets_ReferenceID]
    ON [spec].[Sheets]([ReferenceID] ASC);

