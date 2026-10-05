CREATE TABLE [dbo].[TemplateColumnBlocks] (
    [Id]                     INT            IDENTITY (1, 1) NOT NULL,
    [TableTemplateVersionId] INT            NOT NULL,
    [Name]                   NVARCHAR (100) NOT NULL,
    [DisplayOrder]           INT            NOT NULL,
    [MinInstances]           INT            NOT NULL,
    [MaxInstances]           INT            NULL,
    [InitialInstances]       INT            NOT NULL,
    [StickyColumnCount]      INT            DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TemplateColumnBlocks] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_TemplateColumnBlocks_Instances] CHECK ([MinInstances]>=(0) AND [InitialInstances]>=[MinInstances] AND ([MaxInstances] IS NULL OR [MaxInstances]>=(1) AND [MaxInstances]>=[InitialInstances])),
    CONSTRAINT [FK_TemplateColumnBlocks_TableTemplateVersions_TableTemplateVersionId] FOREIGN KEY ([TableTemplateVersionId]) REFERENCES [dbo].[TableTemplateVersions] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_TemplateColumnBlocks_TableTemplateVersionId]
    ON [dbo].[TemplateColumnBlocks]([TableTemplateVersionId] ASC);


GO

