CREATE TABLE [values].[TextValues] (
    [TextValueID]          INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableCellID] INT NOT NULL,
    [Value]                NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_TextValues] PRIMARY KEY CLUSTERED ([TextValueID] ASC),
    CONSTRAINT [FK_TextValues_SpecSheetTableCells] FOREIGN KEY ([SpecSheetTableCellID]) REFERENCES [dbo].[SpecSheetTableCells] ([SpecSheetTableCellID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TextValues_SpecSheetTableCellID]
    ON [values].[TextValues]([SpecSheetTableCellID] ASC);

