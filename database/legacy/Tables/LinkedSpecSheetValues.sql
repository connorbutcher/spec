CREATE TABLE [legacy].[LinkedSpecSheetValues] (
    [LinkedSpecSheetValueID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableCellID]   INT NOT NULL,
    [LinkedSpecSheetTypeID]  TINYINT NULL,
    [LinkedSpecSheetPhaseID] SMALLINT NULL,
    [TableReferenceID]       UNIQUEIDENTIFIER NULL,
    [LinkedReferenceID]      UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_LinkedSpecSheetValues] PRIMARY KEY CLUSTERED ([LinkedSpecSheetValueID] ASC),
    CONSTRAINT [FK_LinkedSpecSheetValues_SpecSheetTableCells] FOREIGN KEY ([SpecSheetTableCellID]) REFERENCES [legacy].[SpecSheetTableCells] ([SpecSheetTableCellID]),
    CONSTRAINT [FK_LinkedSpecSheetValues_SpecSheetTypeID] FOREIGN KEY ([LinkedSpecSheetTypeID]) REFERENCES [types].[SpecSheetTypes] ([SpecSheetTypeID]),
    CONSTRAINT [FK_LinkedSpecSheetValues_SpecSheetPhasesID] FOREIGN KEY ([LinkedSpecSheetPhaseID]) REFERENCES [types].[SpecSheetPhases] ([SpecSheetPhaseID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_LinkedSpecSheetValues_SpecSheetTableCellID]
    ON [legacy].[LinkedSpecSheetValues]([SpecSheetTableCellID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_LinkedSpecSheetValues_LinkedSpecSheetTypeID]
    ON [legacy].[LinkedSpecSheetValues]([LinkedSpecSheetTypeID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_LinkedSpecSheetValues_LinkedSpecSheetPhaseID]
    ON [legacy].[LinkedSpecSheetValues]([LinkedSpecSheetPhaseID] ASC);

