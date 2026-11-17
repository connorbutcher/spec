CREATE TABLE [template].[TableTemplateWidgets] (
    [TableTemplateWidgetID] SMALLINT IDENTITY (1, 1) NOT NULL,
    [TableTemplateID]       TINYINT NOT NULL,
    [WidgetTypeID]          TINYINT NOT NULL,
    CONSTRAINT [PK_TableTemplateWidgets] PRIMARY KEY CLUSTERED ([TableTemplateWidgetID] ASC),
    CONSTRAINT [FK_TableTemplateWidgets_TableTemplates] FOREIGN KEY ([TableTemplateID]) REFERENCES [template].[TableTemplates] ([TableTemplateID]),
    CONSTRAINT [FK_TableTemplateWidgets_WidgetTypes] FOREIGN KEY ([WidgetTypeID]) REFERENCES [types].[WidgetTypes] ([WidgetTypeID])
);

GO
CREATE NONCLUSTERED INDEX [IX_TableTemplateWidgets_TableTemplateID]
    ON [template].[TableTemplateWidgets]([TableTemplateID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_TableTemplateWidgets_WidgetTypeID]
    ON [template].[TableTemplateWidgets]([WidgetTypeID] ASC);

