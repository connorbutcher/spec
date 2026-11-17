CREATE TABLE [template].[TableGroupTemplateWidgets] (
    [TableGroupTemplateWidgetID] SMALLINT IDENTITY (1, 1) NOT NULL,
    [TableGroupTemplateID]       INT NOT NULL,
    [WidgetTypeID]               TINYINT NOT NULL,
    CONSTRAINT [PK_TableGroupTemplateWidgets] PRIMARY KEY CLUSTERED ([TableGroupTemplateWidgetID] ASC),
    CONSTRAINT [FK_TableGroupTemplateWidgets_TableGroupTemplates] FOREIGN KEY ([TableGroupTemplateID]) REFERENCES [template].[TableGroupTemplates] ([TableGroupTemplateID]),
    CONSTRAINT [FK_TableGroupTemplateWidgets_WidgetTypes] FOREIGN KEY ([WidgetTypeID]) REFERENCES [types].[WidgetTypes] ([WidgetTypeID])
);

GO
CREATE NONCLUSTERED INDEX [IX_TableGroupTemplateWidgets_TableGroupTemplateID]
    ON [template].[TableGroupTemplateWidgets]([TableGroupTemplateID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_TableGroupTemplateWidgets_WidgetTypeID]
    ON [template].[TableGroupTemplateWidgets]([WidgetTypeID] ASC);

