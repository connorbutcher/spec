CREATE TABLE [values].[NumericValues] (
    [SheetRowRevisionId] INT              NOT NULL,
    [SheetCellId]        INT              NOT NULL,
    [Value]              DECIMAL (28, 10) NOT NULL,
    CONSTRAINT [PK_NumericValues] PRIMARY KEY CLUSTERED ([SheetRowRevisionId] ASC, [SheetCellId] ASC),
    CONSTRAINT [FK_NumericValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [dbo].[SheetCells] ([Id]),
    CONSTRAINT [FK_NumericValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [dbo].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_NumericValues_SheetCellId]
    ON [values].[NumericValues]([SheetCellId] ASC);


GO

