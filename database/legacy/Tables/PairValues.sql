CREATE TABLE [legacy].[PairValues] (
    [PairValueID]          INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableCellID] INT NOT NULL,
    [PairValueX]           NVARCHAR (MAX) NULL,
    [PairValueY]           NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_PairValues] PRIMARY KEY CLUSTERED ([PairValueID] ASC),
    CONSTRAINT [FK_PairValues_SpecSheetTableCells] FOREIGN KEY ([SpecSheetTableCellID]) REFERENCES [legacy].[SpecSheetTableCells] ([SpecSheetTableCellID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PairValues_SpecSheetTableCellID]
    ON [legacy].[PairValues]([SpecSheetTableCellID] ASC);

