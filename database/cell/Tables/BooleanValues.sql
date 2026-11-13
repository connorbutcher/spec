CREATE TABLE [cell].[BooleanValues] (
    [SheetRowRevisionId] INT NOT NULL,
    [SheetCellId]        INT NOT NULL,
    [Value]              BIT NOT NULL,
    CONSTRAINT [PK_BooleanValues] PRIMARY KEY CLUSTERED ([SheetRowRevisionId] ASC, [SheetCellId] ASC),
    CONSTRAINT [FK_BooleanValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [spec].[SheetCells] ([Id]),
    CONSTRAINT [FK_BooleanValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [spec].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_BooleanValues_SheetCellId]
    ON [cell].[BooleanValues]([SheetCellId] ASC);


GO

