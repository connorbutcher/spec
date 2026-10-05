CREATE TABLE [values].[DateValues] (
    [Id]                 INT  IDENTITY (1, 1) NOT NULL,
    [SheetRowRevisionId] INT  NOT NULL,
    [SheetCellId]        INT  NOT NULL,
    [Value]              DATE NOT NULL,
    CONSTRAINT [PK_DateValues] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DateValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [dbo].[SheetCells] ([Id]),
    CONSTRAINT [FK_DateValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [dbo].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_DateValues_SheetCellId]
    ON [values].[DateValues]([SheetCellId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_DateValues_SheetRowRevisionId_SheetCellId]
    ON [values].[DateValues]([SheetRowRevisionId] ASC, [SheetCellId] ASC);


GO

