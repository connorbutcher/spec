CREATE TABLE [values].[BooleanValues] (
    [SheetRowRevisionId] INT NOT NULL,
    [SheetCellId]        INT NOT NULL,
    [Value]              BIT NOT NULL,
    CONSTRAINT [PK_BooleanValues] PRIMARY KEY CLUSTERED ([SheetRowRevisionId] ASC, [SheetCellId] ASC),
    CONSTRAINT [FK_BooleanValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [dbo].[SheetCells] ([Id]),
    CONSTRAINT [FK_BooleanValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [dbo].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_BooleanValues_SheetCellId]
    ON [values].[BooleanValues]([SheetCellId] ASC);


GO

