CREATE TABLE [cell].[TextValues] (
    [SheetRowRevisionId] INT             NOT NULL,
    [SheetCellId]        INT             NOT NULL,
    [Value]              NVARCHAR (4000) NOT NULL,
    [LookupValue]        AS              (CONVERT([nvarchar](200),left([Value],(200)))) PERSISTED,
    CONSTRAINT [PK_TextValues] PRIMARY KEY CLUSTERED ([SheetRowRevisionId] ASC, [SheetCellId] ASC),
    CONSTRAINT [FK_TextValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [spec].[SheetCells] ([Id]),
    CONSTRAINT [FK_TextValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [spec].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_TextValues_LookupValue]
    ON [cell].[TextValues]([LookupValue] ASC)
    INCLUDE([SheetCellId], [SheetRowRevisionId]);


GO

CREATE NONCLUSTERED INDEX [IX_TextValues_SheetCellId]
    ON [cell].[TextValues]([SheetCellId] ASC);


GO

