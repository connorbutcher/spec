CREATE TABLE [dbo].[TableTemplateVersions] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [TableTemplateId]   INT           NOT NULL,
    [VersionNumber]     INT           NOT NULL,
    [Orientation]       NVARCHAR (20) NOT NULL,
    [CreatedAtUtc]      DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [StickyColumnCount] INT           DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TableTemplateVersions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_TableTemplateVersions_StickyColumnCount] CHECK ([StickyColumnCount]>=(0)),
    CONSTRAINT [CK_TableTemplateVersions_VersionNumber] CHECK ([VersionNumber]>=(1)),
    CONSTRAINT [FK_TableTemplateVersions_TableTemplates_TableTemplateId] FOREIGN KEY ([TableTemplateId]) REFERENCES [dbo].[TableTemplates] ([Id]) ON DELETE CASCADE
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_TableTemplateVersions_TableTemplateId_VersionNumber]
    ON [dbo].[TableTemplateVersions]([TableTemplateId] ASC, [VersionNumber] ASC);


GO

