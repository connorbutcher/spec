CREATE TABLE [template].[TableCellTemplates] (
    [TableCellTemplateID] INT IDENTITY (1, 1) NOT NULL,
    [TableRowTemplateID]  INT NOT NULL,
    [TableCellTypeID]     TINYINT NOT NULL,
    [CellConfigurationID] INT NOT NULL,
    [ColumnNumber]        TINYINT NOT NULL,
    [ColSpan]             TINYINT NOT NULL,
    [RowSpan]             TINYINT NOT NULL,
    [GroupKey]            VARCHAR (64) NULL,
    CONSTRAINT [PK_TableCellTemplates] PRIMARY KEY CLUSTERED ([TableCellTemplateID] ASC),
    CONSTRAINT [FK_TableCellTemplates_TableRowTemplates] FOREIGN KEY ([TableRowTemplateID]) REFERENCES [template].[TableRowTemplates] ([TableRowTemplateID]),
    CONSTRAINT [FK_TableCellTemplates_TableCellTypes] FOREIGN KEY ([TableCellTypeID]) REFERENCES [template].[TableCellTypes] ([TableCellTypeID]),
    CONSTRAINT [FK_TableCellTemplates_CellConfigurations] FOREIGN KEY ([CellConfigurationID]) REFERENCES [configuration].[CellConfigurations] ([CellConfigurationID])
);

GO
CREATE NONCLUSTERED INDEX [IX_TableCellTemplates_TableRowTemplateID]
    ON [template].[TableCellTemplates]([TableRowTemplateID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_TableCellTemplates_TableCellTypeID]
    ON [template].[TableCellTemplates]([TableCellTypeID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_TableCellTemplates_CellConfigurationID]
    ON [template].[TableCellTemplates]([CellConfigurationID] ASC);

