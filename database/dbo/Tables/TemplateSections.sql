CREATE TABLE [dbo].[TemplateSections] (
    [Id]                     INT            IDENTITY (1, 1) NOT NULL,
    [TableTemplateVersionId] INT            NOT NULL,
    [ParentSectionId]        INT            NULL,
    [Name]                   NVARCHAR (100) NOT NULL,
    [DisplayOrder]           INT            NOT NULL,
    [Role]                   NVARCHAR (20)  NOT NULL,
    [MinInstances]           INT            DEFAULT ((0)) NOT NULL,
    [MaxInstances]           INT            NULL,
    [InitialInstances]       INT            DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TemplateSections] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_TemplateSections_Header] CHECK ([Role]<>'Header' OR [ParentSectionId] IS NULL AND [MinInstances]=(1) AND [MaxInstances]=(1) AND [InitialInstances]=(1)),
    CONSTRAINT [CK_TemplateSections_Instances] CHECK ([MinInstances]>=(0) AND [InitialInstances]>=[MinInstances] AND ([MaxInstances] IS NULL OR [MaxInstances]>=(1) AND [MaxInstances]>=[InitialInstances])),
    CONSTRAINT [FK_TemplateSections_TableTemplateVersions_TableTemplateVersionId] FOREIGN KEY ([TableTemplateVersionId]) REFERENCES [dbo].[TableTemplateVersions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TemplateSections_TemplateSections_ParentSectionId] FOREIGN KEY ([ParentSectionId]) REFERENCES [dbo].[TemplateSections] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_TemplateSections_ParentSectionId]
    ON [dbo].[TemplateSections]([ParentSectionId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_TemplateSections_OneHeaderPerVersion]
    ON [dbo].[TemplateSections]([TableTemplateVersionId] ASC) WHERE ([Role]='Header');


GO

CREATE NONCLUSTERED INDEX [IX_TemplateSections_TableTemplateVersionId]
    ON [dbo].[TemplateSections]([TableTemplateVersionId] ASC);


GO

