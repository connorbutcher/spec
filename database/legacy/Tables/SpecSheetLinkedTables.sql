CREATE TABLE [legacy].[SpecSheetLinkedTables] (
    [SpecSheetLinkedTableID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetID]            INT NOT NULL,
    [TableReferenceID]       UNIQUEIDENTIFIER NOT NULL,
    [DisplayOrder]           SMALLINT NOT NULL,
    [LinkedByUserID]         SMALLINT NOT NULL,
    [LinkedDateTime]         DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_SpecSheetLinkedTables] PRIMARY KEY CLUSTERED ([SpecSheetLinkedTableID] ASC),
    CONSTRAINT [FK_SpecSheetLinkedTables_SpecSheets] FOREIGN KEY ([SpecSheetID]) REFERENCES [legacy].[SpecSheets] ([SpecSheetID]),
    CONSTRAINT [FK_SpecSheetLinkedTables_Users_LinkedByUserID] FOREIGN KEY ([LinkedByUserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetLinkedTables_SpecSheetID]
    ON [legacy].[SpecSheetLinkedTables]([SpecSheetID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetLinkedTables_LinkedByUserID]
    ON [legacy].[SpecSheetLinkedTables]([LinkedByUserID] ASC);

