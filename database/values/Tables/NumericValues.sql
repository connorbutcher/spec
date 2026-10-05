CREATE TABLE [values].[NumericValues] (
    [Id]                 INT              IDENTITY (1, 1) NOT NULL,
    [SheetRowRevisionId] INT              NOT NULL,
    [SheetCellId]        INT              NOT NULL,
    [Value]              DECIMAL (28, 10) NOT NULL,
    CONSTRAINT [PK_NumericValues] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_NumericValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [dbo].[SheetCells] ([Id]),
    CONSTRAINT [FK_NumericValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [dbo].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_NumericValues_SheetCellId]
    ON [values].[NumericValues]([SheetCellId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_NumericValues_SheetRowRevisionId_SheetCellId]
    ON [values].[NumericValues]([SheetRowRevisionId] ASC, [SheetCellId] ASC);


GO

