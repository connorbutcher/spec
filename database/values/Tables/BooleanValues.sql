CREATE TABLE [values].[BooleanValues] (
    [Id]                 INT IDENTITY (1, 1) NOT NULL,
    [SheetRowRevisionId] INT NOT NULL,
    [SheetCellId]        INT NOT NULL,
    [Value]              BIT NOT NULL,
    CONSTRAINT [PK_BooleanValues] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_BooleanValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [dbo].[SheetCells] ([Id]),
    CONSTRAINT [FK_BooleanValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [dbo].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_BooleanValues_SheetCellId]
    ON [values].[BooleanValues]([SheetCellId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_BooleanValues_SheetRowRevisionId_SheetCellId]
    ON [values].[BooleanValues]([SheetRowRevisionId] ASC, [SheetCellId] ASC);


GO

