CREATE TABLE [design].[TableTemplateWidgets] (
    [Id]                     INT IDENTITY (1, 1) NOT NULL,
    [TableTemplateVersionId] INT NOT NULL,
    [WidgetTypeId]           INT NOT NULL,
    CONSTRAINT [PK_TableTemplateWidgets] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TableTemplateWidgets_TableTemplateVersions_TableTemplateVersionId] FOREIGN KEY ([TableTemplateVersionId]) REFERENCES [design].[TableTemplateVersions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TableTemplateWidgets_WidgetTypes_WidgetTypeId] FOREIGN KEY ([WidgetTypeId]) REFERENCES [design].[WidgetTypes] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_TableTemplateWidgets_TableTemplateVersionId]
    ON [design].[TableTemplateWidgets]([TableTemplateVersionId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_TableTemplateWidgets_WidgetTypeId]
    ON [design].[TableTemplateWidgets]([WidgetTypeId] ASC);


GO
