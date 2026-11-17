CREATE TABLE [values].[PairValues] (
    [PairValueID]          INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableCellID] INT NOT NULL,
    [PairValueX]           NVARCHAR (MAX) NULL,
    [PairValueY]           NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_PairValues] PRIMARY KEY CLUSTERED ([PairValueID] ASC),
    CONSTRAINT [FK_PairValues_SpecSheetTableCells] FOREIGN KEY ([SpecSheetTableCellID]) REFERENCES [dbo].[SpecSheetTableCells] ([SpecSheetTableCellID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PairValues_SpecSheetTableCellID]
    ON [values].[PairValues]([SpecSheetTableCellID] ASC);

