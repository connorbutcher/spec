CREATE TABLE [cell].[PairValues] (
    [SheetTableRowRevisionID] INT            NOT NULL,
    [SheetTableCellID]        INT            NOT NULL,
    [PairValueX]              NVARCHAR (MAX) NULL,
    [PairValueY]              NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_PairValues] PRIMARY KEY CLUSTERED ([SheetTableRowRevisionID] ASC, [SheetTableCellID] ASC),
    CONSTRAINT [CK_PairValues_HasValue] CHECK ([PairValueX] IS NOT NULL OR [PairValueY] IS NOT NULL),
    CONSTRAINT [FK_PairValues_SheetTableRowRevisions_SheetTableRowRevisionID] FOREIGN KEY ([SheetTableRowRevisionID]) REFERENCES [spec].[SheetTableRowRevisions] ([SheetTableRowRevisionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_PairValues_SheetTableCells_SheetTableCellID] FOREIGN KEY ([SheetTableCellID]) REFERENCES [spec].[SheetTableCells] ([SheetTableCellID])
);

GO
CREATE NONCLUSTERED INDEX [IX_PairValues_SheetTableCellID]
    ON [cell].[PairValues]([SheetTableCellID] ASC);

