CREATE TABLE [values].[CellSettings] (
    [SheetRowRevisionId] INT            NOT NULL,
    [SheetCellId]        INT            NOT NULL,
    [Settings]           NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_CellSettings] PRIMARY KEY CLUSTERED ([SheetRowRevisionId] ASC, [SheetCellId] ASC),
    CONSTRAINT [FK_CellSettings_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [dbo].[SheetCells] ([Id]),
    CONSTRAINT [FK_CellSettings_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [dbo].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_CellSettings_SheetCellId]
    ON [values].[CellSettings]([SheetCellId] ASC);


GO

