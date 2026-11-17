CREATE TABLE [spec].[SheetTables] (
    [SheetTableID]    INT              IDENTITY (1, 1) NOT NULL,
    [SheetID]         INT              NOT NULL,
    [TableTemplateID] TINYINT          NOT NULL,
    [ReferenceID]     UNIQUEIDENTIFIER CONSTRAINT [DF_SheetTables_ReferenceID] DEFAULT (newsequentialid()) NOT NULL,
    [CreatedDateTime] DATETIME2 (7)    CONSTRAINT [DF_SheetTables_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetTables] PRIMARY KEY CLUSTERED ([SheetTableID] ASC),
    CONSTRAINT [FK_SheetTables_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTables_TableTemplates_TableTemplateID] FOREIGN KEY ([TableTemplateID]) REFERENCES [template].[TableTemplates] ([TableTemplateID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTables_SheetID_ReferenceID]
    ON [spec].[SheetTables]([SheetID] ASC, [ReferenceID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTables_TableTemplateID]
    ON [spec].[SheetTables]([TableTemplateID] ASC);

