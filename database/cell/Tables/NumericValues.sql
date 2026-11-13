CREATE TABLE [cell].[NumericValues] (
    [SheetRowRevisionId] INT              NOT NULL,
    [SheetCellId]        INT              NOT NULL,
    [Value]              DECIMAL (28, 10) NOT NULL,
    CONSTRAINT [PK_NumericValues] PRIMARY KEY CLUSTERED ([SheetRowRevisionId] ASC, [SheetCellId] ASC),
    CONSTRAINT [FK_NumericValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [spec].[SheetCells] ([Id]),
    CONSTRAINT [FK_NumericValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [spec].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_NumericValues_SheetCellId]
    ON [cell].[NumericValues]([SheetCellId] ASC);


GO

