CREATE TABLE [cell].[DateValues] (
    [SheetRowRevisionId] INT  NOT NULL,
    [SheetCellId]        INT  NOT NULL,
    [Value]              DATE NOT NULL,
    CONSTRAINT [PK_DateValues] PRIMARY KEY CLUSTERED ([SheetRowRevisionId] ASC, [SheetCellId] ASC),
    CONSTRAINT [FK_DateValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [spec].[SheetCells] ([Id]),
    CONSTRAINT [FK_DateValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [spec].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_DateValues_SheetCellId]
    ON [cell].[DateValues]([SheetCellId] ASC);


GO

