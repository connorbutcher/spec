CREATE TABLE [spec].[SheetLinkedTables] (
    [SheetLinkedTableID] INT              IDENTITY (1, 1) NOT NULL,
    [SheetID]            INT              NOT NULL,
    [TableReferenceID]   UNIQUEIDENTIFIER NOT NULL,
    [LinkedByUserID]     SMALLINT         NOT NULL,
    [LinkedDateTime]     DATETIME2 (7)    NOT NULL,
    [CreatedDateTime]    DATETIME2 (7)    CONSTRAINT [DF_SheetLinkedTables_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetLinkedTables] PRIMARY KEY CLUSTERED ([SheetLinkedTableID] ASC),
    CONSTRAINT [FK_SheetLinkedTables_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetLinkedTables_Users_LinkedByUserID] FOREIGN KEY ([LinkedByUserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetLinkedTables_SheetID_TableReferenceID]
    ON [spec].[SheetLinkedTables]([SheetID] ASC, [TableReferenceID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetLinkedTables_LinkedByUserID]
    ON [spec].[SheetLinkedTables]([LinkedByUserID] ASC);

