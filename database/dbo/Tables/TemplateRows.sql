CREATE TABLE [dbo].[TemplateRows] (
    [Id]                INT IDENTITY (1, 1) NOT NULL,
    [TemplateSectionId] INT NOT NULL,
    [DisplayOrder]      INT NOT NULL,
    CONSTRAINT [PK_TemplateRows] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TemplateRows_TemplateSections_TemplateSectionId] FOREIGN KEY ([TemplateSectionId]) REFERENCES [dbo].[TemplateSections] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_TemplateRows_TemplateSectionId]
    ON [dbo].[TemplateRows]([TemplateSectionId] ASC);


GO

