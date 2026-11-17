CREATE TABLE [spec].[SheetExternalReferences] (
    [SheetExternalReferenceID] INT              IDENTITY (1, 1) NOT NULL,
    [SheetID]                  INT              NOT NULL,
    [ReferenceID]              UNIQUEIDENTIFIER NOT NULL,
    [CreatedDateTime]          DATETIME2 (7)    CONSTRAINT [DF_SheetExternalReferences_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetExternalReferences] PRIMARY KEY CLUSTERED ([SheetExternalReferenceID] ASC),
    CONSTRAINT [FK_SheetExternalReferences_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]) ON DELETE CASCADE
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetExternalReferences_SheetID_ReferenceID]
    ON [spec].[SheetExternalReferences]([SheetID] ASC, [ReferenceID] ASC);

