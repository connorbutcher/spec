CREATE TABLE [cell].[TextValues] (
    [SheetTableRowRevisionID] INT            NOT NULL,
    [SheetTableCellID]        INT            NOT NULL,
    [Value]                   NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_TextValues] PRIMARY KEY CLUSTERED ([SheetTableRowRevisionID] ASC, [SheetTableCellID] ASC),
    CONSTRAINT [FK_TextValues_SheetTableRowRevisions_SheetTableRowRevisionID] FOREIGN KEY ([SheetTableRowRevisionID]) REFERENCES [spec].[SheetTableRowRevisions] ([SheetTableRowRevisionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_TextValues_SheetTableCells_SheetTableCellID] FOREIGN KEY ([SheetTableCellID]) REFERENCES [spec].[SheetTableCells] ([SheetTableCellID])
);

GO
CREATE NONCLUSTERED INDEX [IX_TextValues_SheetTableCellID]
    ON [cell].[TextValues]([SheetTableCellID] ASC);

