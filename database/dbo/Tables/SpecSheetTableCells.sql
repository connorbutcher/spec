CREATE TABLE [dbo].[SpecSheetTableCells] (
    [SpecSheetTableCellID]  INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableRowID]   INT NOT NULL,
    [TableCellTypeID]       TINYINT NOT NULL,
    [CellConfigurationID]   INT NOT NULL,
    [RowNumber]             TINYINT NOT NULL,
    [ColumnNumber]          SMALLINT NOT NULL,
    [ColSpan]               TINYINT NOT NULL,
    [RowSpan]               TINYINT NOT NULL,
    [ReferenceID]           UNIQUEIDENTIFIER NOT NULL,
    [IsDeleted]             BIT CONSTRAINT [DF_SpecSheetTableCells_IsDeleted] DEFAULT ((0)) NOT NULL,
    [CreatedByUserID]       SMALLINT NOT NULL,
    [CreatedDateTime]       DATETIME2 (0) NOT NULL,
    [LastModifiedByUserID]  SMALLINT NULL,
    [LastModifiedDateTime]  DATETIME2 (0) NULL,
    [ConfigurationOverride] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_SpecSheetTableCells] PRIMARY KEY CLUSTERED ([SpecSheetTableCellID] ASC),
    CONSTRAINT [FK_SpecSheetTableCells_SpecSheetTableRows] FOREIGN KEY ([SpecSheetTableRowID]) REFERENCES [dbo].[SpecSheetTableRows] ([SpecSheetTableRowID]),
    CONSTRAINT [FK_SpecSheetTableCells_TableCellTypes] FOREIGN KEY ([TableCellTypeID]) REFERENCES [template].[TableCellTypes] ([TableCellTypeID]),
    CONSTRAINT [FK_SpecSheetTableCells_CellConfigurations] FOREIGN KEY ([CellConfigurationID]) REFERENCES [configuration].[CellConfigurations] ([CellConfigurationID]),
    CONSTRAINT [FK_SpecSheetTableCells_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SpecSheetTableCells_Users_LastModifiedByUserID] FOREIGN KEY ([LastModifiedByUserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableCells_SpecSheetTableRowID]
    ON [dbo].[SpecSheetTableCells]([SpecSheetTableRowID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableCells_TableCellTypeID]
    ON [dbo].[SpecSheetTableCells]([TableCellTypeID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableCells_CellConfigurationID]
    ON [dbo].[SpecSheetTableCells]([CellConfigurationID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableCells_CreatedByUserID]
    ON [dbo].[SpecSheetTableCells]([CreatedByUserID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableCells_LastModifiedByUserID]
    ON [dbo].[SpecSheetTableCells]([LastModifiedByUserID] ASC);

