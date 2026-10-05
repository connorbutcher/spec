CREATE TABLE [values].[TextValues] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [SheetRowRevisionId] INT             NOT NULL,
    [SheetCellId]        INT             NOT NULL,
    [Value]              NVARCHAR (4000) NOT NULL,
    [LookupValue]        AS              (CONVERT([nvarchar](200),left([Value],(200)))) PERSISTED,
    CONSTRAINT [PK_TextValues] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TextValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [dbo].[SheetCells] ([Id]),
    CONSTRAINT [FK_TextValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [dbo].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_TextValues_LookupValue]
    ON [values].[TextValues]([LookupValue] ASC)
    INCLUDE([SheetCellId], [SheetRowRevisionId]);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_TextValues_SheetRowRevisionId_SheetCellId]
    ON [values].[TextValues]([SheetRowRevisionId] ASC, [SheetCellId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_TextValues_SheetCellId]
    ON [values].[TextValues]([SheetCellId] ASC);


GO

