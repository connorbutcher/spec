CREATE TABLE [dbo].[SheetColumnBlockRevisions] (
    [Id]                 INT           IDENTITY (1, 1) NOT NULL,
    [SheetColumnBlockId] INT           NOT NULL,
    [RevisionNumber]     INT           NOT NULL,
    [Status]             INT           NOT NULL,
    [DisplayOrder]       INT           NOT NULL,
    [IsDeleted]          BIT           NOT NULL,
    [AuthorUserId]       INT           NOT NULL,
    [CreatedAtUtc]       DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAtUtc]       DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedAtUtc]     DATETIME2 (7) NULL,
    [SupersededAtUtc]    DATETIME2 (7) NULL,
    [RowVersion]         ROWVERSION    NOT NULL,
    [SheetVersionId]     INT           NULL,
    CONSTRAINT [PK_SheetColumnBlockRevisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetColumnBlockRevisions_PublishedAt] CHECK ([Status]=(0) AND [PublishedAtUtc] IS NULL OR [Status]=(1) AND [PublishedAtUtc] IS NOT NULL),
    CONSTRAINT [CK_SheetColumnBlockRevisions_SupersededAt] CHECK ([SupersededAtUtc] IS NULL OR [Status]=(1) AND [SupersededAtUtc]>=[PublishedAtUtc]),
    CONSTRAINT [FK_SheetColumnBlockRevisions_SheetColumnBlocks_SheetColumnBlockId] FOREIGN KEY ([SheetColumnBlockId]) REFERENCES [dbo].[SheetColumnBlocks] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetColumnBlockRevisions_SheetVersions_SheetVersionId] FOREIGN KEY ([SheetVersionId]) REFERENCES [dbo].[SheetVersions] ([Id]),
    CONSTRAINT [FK_SheetColumnBlockRevisions_Users_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [dbo].[Users] ([Id])
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetColumnBlockRevisions_SheetColumnBlockId_RevisionNumber]
    ON [dbo].[SheetColumnBlockRevisions]([SheetColumnBlockId] ASC, [RevisionNumber] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetColumnBlockRevisions_AuthorUserId_Status]
    ON [dbo].[SheetColumnBlockRevisions]([AuthorUserId] ASC, [Status] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetColumnBlockRevisions_OneDraftPerBlock]
    ON [dbo].[SheetColumnBlockRevisions]([SheetColumnBlockId] ASC) WHERE ([Status]=(0));


GO

CREATE NONCLUSTERED INDEX [IX_SheetColumnBlockRevisions_SheetVersionId]
    ON [dbo].[SheetColumnBlockRevisions]([SheetVersionId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetColumnBlockRevisions_OneCurrentPerBlock]
    ON [dbo].[SheetColumnBlockRevisions]([SheetColumnBlockId] ASC) WHERE ([Status]=(1) AND [SupersededAtUtc] IS NULL);


GO

CREATE NONCLUSTERED INDEX [IX_SheetColumnBlockRevisions_Published]
    ON [dbo].[SheetColumnBlockRevisions]([SheetColumnBlockId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc]) WHERE ([Status]=(1));


GO

