CREATE TABLE [cell].[CellConfigurationOverrides] (
    [SheetTableRowRevisionID] INT            NOT NULL,
    [SheetTableCellID]        INT            NOT NULL,
    [ConfigurationOverride]   NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_CellConfigurationOverrides] PRIMARY KEY CLUSTERED ([SheetTableRowRevisionID] ASC, [SheetTableCellID] ASC),
    CONSTRAINT [CK_CellConfigurationOverrides_ConfigurationOverride] CHECK (isjson([ConfigurationOverride])=(1)),
    CONSTRAINT [FK_CellConfigurationOverrides_SheetTableRowRevisions_SheetTableRowRevisionID] FOREIGN KEY ([SheetTableRowRevisionID]) REFERENCES [spec].[SheetTableRowRevisions] ([SheetTableRowRevisionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_CellConfigurationOverrides_SheetTableCells_SheetTableCellID] FOREIGN KEY ([SheetTableCellID]) REFERENCES [spec].[SheetTableCells] ([SheetTableCellID])
);

GO
CREATE NONCLUSTERED INDEX [IX_CellConfigurationOverrides_SheetTableCellID]
    ON [cell].[CellConfigurationOverrides]([SheetTableCellID] ASC);

