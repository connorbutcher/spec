CREATE TABLE [dbo].[SpecSheetTableWidgets] (
    [SpecSheetTableWidgetID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableID]       INT NOT NULL,
    [TableTemplateWidgetID]  SMALLINT NOT NULL,
    CONSTRAINT [PK_SpecSheetTableWidgets] PRIMARY KEY CLUSTERED ([SpecSheetTableWidgetID] ASC),
    CONSTRAINT [FK_SpecSheetTableWidgets_SpecSheetTables] FOREIGN KEY ([SpecSheetTableID]) REFERENCES [dbo].[SpecSheetTables] ([SpecSheetTableID]),
    CONSTRAINT [FK_SpecSheetTableWidgets_TableTemplateWidgets] FOREIGN KEY ([TableTemplateWidgetID]) REFERENCES [template].[TableTemplateWidgets] ([TableTemplateWidgetID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableWidgets_SpecSheetTableID]
    ON [dbo].[SpecSheetTableWidgets]([SpecSheetTableID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableWidgets_TableTemplateWidgetID]
    ON [dbo].[SpecSheetTableWidgets]([TableTemplateWidgetID] ASC);

