CREATE TABLE [cell].[LinkedSheetValues] (
    [SheetTableRowRevisionID] INT              NOT NULL,
    [SheetTableCellID]        INT              NOT NULL,
    [LinkedSpecSheetTypeID]   TINYINT          NULL,
    [LinkedSpecSheetPhaseID]  SMALLINT         NULL,
    [TableReferenceID]        UNIQUEIDENTIFIER NULL,
    [LinkedReferenceID]       UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_LinkedSheetValues] PRIMARY KEY CLUSTERED ([SheetTableRowRevisionID] ASC, [SheetTableCellID] ASC),
    CONSTRAINT [FK_LinkedSheetValues_SheetTableRowRevisions_SheetTableRowRevisionID] FOREIGN KEY ([SheetTableRowRevisionID]) REFERENCES [spec].[SheetTableRowRevisions] ([SheetTableRowRevisionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_LinkedSheetValues_SheetTableCells_SheetTableCellID] FOREIGN KEY ([SheetTableCellID]) REFERENCES [spec].[SheetTableCells] ([SheetTableCellID]),
    CONSTRAINT [FK_LinkedSheetValues_SpecSheetTypes_LinkedSpecSheetTypeID] FOREIGN KEY ([LinkedSpecSheetTypeID]) REFERENCES [types].[SpecSheetTypes] ([SpecSheetTypeID]),
    CONSTRAINT [FK_LinkedSheetValues_SpecSheetPhases_LinkedSpecSheetPhaseID] FOREIGN KEY ([LinkedSpecSheetPhaseID]) REFERENCES [types].[SpecSheetPhases] ([SpecSheetPhaseID])
);

GO
CREATE NONCLUSTERED INDEX [IX_LinkedSheetValues_SheetTableCellID]
    ON [cell].[LinkedSheetValues]([SheetTableCellID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_LinkedSheetValues_LinkedSpecSheetTypeID]
    ON [cell].[LinkedSheetValues]([LinkedSpecSheetTypeID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_LinkedSheetValues_LinkedSpecSheetPhaseID]
    ON [cell].[LinkedSheetValues]([LinkedSpecSheetPhaseID] ASC);

