CREATE TABLE [cell].[NumericValues] (
    [SheetTableRowRevisionID] INT             NOT NULL,
    [SheetTableCellID]        INT             NOT NULL,
    [Value]                   DECIMAL (18, 9) NOT NULL,
    CONSTRAINT [PK_NumericValues] PRIMARY KEY CLUSTERED ([SheetTableRowRevisionID] ASC, [SheetTableCellID] ASC),
    CONSTRAINT [FK_NumericValues_SheetTableRowRevisions_SheetTableRowRevisionID] FOREIGN KEY ([SheetTableRowRevisionID]) REFERENCES [spec].[SheetTableRowRevisions] ([SheetTableRowRevisionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_NumericValues_SheetTableCells_SheetTableCellID] FOREIGN KEY ([SheetTableCellID]) REFERENCES [spec].[SheetTableCells] ([SheetTableCellID])
);

GO
CREATE NONCLUSTERED INDEX [IX_NumericValues_SheetTableCellID]
    ON [cell].[NumericValues]([SheetTableCellID] ASC);

