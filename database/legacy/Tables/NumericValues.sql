CREATE TABLE [legacy].[NumericValues] (
    [NumericValueID]       INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableCellID] INT NOT NULL,
    [Value]                DECIMAL (18, 9) NULL,
    CONSTRAINT [PK_NumericValues] PRIMARY KEY CLUSTERED ([NumericValueID] ASC),
    CONSTRAINT [FK_NumericValues_SpecSheetTableCells] FOREIGN KEY ([SpecSheetTableCellID]) REFERENCES [legacy].[SpecSheetTableCells] ([SpecSheetTableCellID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_NumericValues_SpecSheetTableCellID]
    ON [legacy].[NumericValues]([SpecSheetTableCellID] ASC);

