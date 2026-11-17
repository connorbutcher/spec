CREATE TABLE [legacy].[SpecSheetTableWidgets] (
    [SpecSheetTableWidgetID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableID]       INT NOT NULL,
    [TableTemplateWidgetID]  SMALLINT NOT NULL,
    CONSTRAINT [PK_SpecSheetTableWidgets] PRIMARY KEY CLUSTERED ([SpecSheetTableWidgetID] ASC),
    CONSTRAINT [FK_SpecSheetTableWidgets_SpecSheetTables] FOREIGN KEY ([SpecSheetTableID]) REFERENCES [legacy].[SpecSheetTables] ([SpecSheetTableID]),
    CONSTRAINT [FK_SpecSheetTableWidgets_TableTemplateWidgets] FOREIGN KEY ([TableTemplateWidgetID]) REFERENCES [template].[TableTemplateWidgets] ([TableTemplateWidgetID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableWidgets_SpecSheetTableID]
    ON [legacy].[SpecSheetTableWidgets]([SpecSheetTableID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableWidgets_TableTemplateWidgetID]
    ON [legacy].[SpecSheetTableWidgets]([TableTemplateWidgetID] ASC);

