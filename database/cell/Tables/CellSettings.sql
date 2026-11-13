CREATE TABLE [cell].[CellSettings] (
    [SheetRowRevisionId] INT            NOT NULL,
    [SheetCellId]        INT            NOT NULL,
    [Settings]           NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_CellSettings] PRIMARY KEY CLUSTERED ([SheetRowRevisionId] ASC, [SheetCellId] ASC),
    CONSTRAINT [FK_CellSettings_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [spec].[SheetCells] ([Id]),
    CONSTRAINT [FK_CellSettings_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [spec].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_CellSettings_SheetCellId]
    ON [cell].[CellSettings]([SheetCellId] ASC);


GO

