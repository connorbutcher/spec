CREATE TABLE [cell].[OptionValues] (
    [SheetRowRevisionId] INT NOT NULL,
    [SheetCellId]        INT NOT NULL,
    [CellTypeOptionId]   INT NOT NULL,
    CONSTRAINT [PK_OptionValues] PRIMARY KEY CLUSTERED ([SheetRowRevisionId] ASC, [SheetCellId] ASC),
    CONSTRAINT [FK_OptionValues_CellTypeOptions_CellTypeOptionId] FOREIGN KEY ([CellTypeOptionId]) REFERENCES [design].[CellTypeOptions] ([Id]),
    CONSTRAINT [FK_OptionValues_SheetCells_SheetCellId] FOREIGN KEY ([SheetCellId]) REFERENCES [spec].[SheetCells] ([Id]),
    CONSTRAINT [FK_OptionValues_SheetRowRevisions_SheetRowRevisionId] FOREIGN KEY ([SheetRowRevisionId]) REFERENCES [spec].[SheetRowRevisions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_OptionValues_SheetCellId]
    ON [cell].[OptionValues]([SheetCellId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_OptionValues_CellTypeOptionId]
    ON [cell].[OptionValues]([CellTypeOptionId] ASC);


GO

