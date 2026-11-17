CREATE TABLE [design].[TemplateSectionWidgets] (
    [Id]                INT IDENTITY (1, 1) NOT NULL,
    [TemplateSectionId] INT NOT NULL,
    [WidgetTypeId]      INT NOT NULL,
    CONSTRAINT [PK_TemplateSectionWidgets] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TemplateSectionWidgets_TemplateSections_TemplateSectionId] FOREIGN KEY ([TemplateSectionId]) REFERENCES [design].[TemplateSections] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TemplateSectionWidgets_WidgetTypes_WidgetTypeId] FOREIGN KEY ([WidgetTypeId]) REFERENCES [design].[WidgetTypes] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_TemplateSectionWidgets_TemplateSectionId]
    ON [design].[TemplateSectionWidgets]([TemplateSectionId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_TemplateSectionWidgets_WidgetTypeId]
    ON [design].[TemplateSectionWidgets]([WidgetTypeId] ASC);


GO
