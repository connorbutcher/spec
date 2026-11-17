CREATE TABLE [dbo].[SpecSheetLinkedTables] (
    [SpecSheetLinkedTableID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetID]            INT NOT NULL,
    [TableReferenceID]       UNIQUEIDENTIFIER NOT NULL,
    [DisplayOrder]           SMALLINT NOT NULL,
    [LinkedByUserID]         SMALLINT NOT NULL,
    [LinkedDateTime]         DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_SpecSheetLinkedTables] PRIMARY KEY CLUSTERED ([SpecSheetLinkedTableID] ASC),
    CONSTRAINT [FK_SpecSheetLinkedTables_SpecSheets] FOREIGN KEY ([SpecSheetID]) REFERENCES [dbo].[SpecSheets] ([SpecSheetID]),
    CONSTRAINT [FK_SpecSheetLinkedTables_Users_LinkedByUserID] FOREIGN KEY ([LinkedByUserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetLinkedTables_SpecSheetID]
    ON [dbo].[SpecSheetLinkedTables]([SpecSheetID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetLinkedTables_LinkedByUserID]
    ON [dbo].[SpecSheetLinkedTables]([LinkedByUserID] ASC);

