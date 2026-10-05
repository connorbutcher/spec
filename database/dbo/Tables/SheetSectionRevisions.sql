CREATE TABLE [dbo].[SheetSectionRevisions] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [SheetSectionId]  INT           NOT NULL,
    [RevisionNumber]  INT           NOT NULL,
    [Status]          INT           NOT NULL,
    [DisplayOrder]    INT           NOT NULL,
    [IsDeleted]       BIT           NOT NULL,
    [AuthorUserId]    INT           NOT NULL,
    [CreatedAtUtc]    DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAtUtc]    DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedAtUtc]  DATETIME2 (7) NULL,
    [SheetVersionId]  INT           NULL,
    [RowVersion]      ROWVERSION    NOT NULL,
    [SupersededAtUtc] DATETIME2 (7) NULL,
    CONSTRAINT [PK_SheetSectionRevisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetSectionRevisions_PublishedAt] CHECK ([Status]=(0) AND [PublishedAtUtc] IS NULL OR [Status]=(1) AND [PublishedAtUtc] IS NOT NULL),
    CONSTRAINT [CK_SheetSectionRevisions_SupersededAt] CHECK ([SupersededAtUtc] IS NULL OR [Status]=(1) AND [SupersededAtUtc]>=[PublishedAtUtc]),
    CONSTRAINT [FK_SheetSectionRevisions_SheetSections_SheetSectionId] FOREIGN KEY ([SheetSectionId]) REFERENCES [dbo].[SheetSections] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetSectionRevisions_SheetVersions_SheetVersionId] FOREIGN KEY ([SheetVersionId]) REFERENCES [dbo].[SheetVersions] ([Id]),
    CONSTRAINT [FK_SheetSectionRevisions_Users_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [dbo].[Users] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSectionRevisions_Published]
    ON [dbo].[SheetSectionRevisions]([SheetSectionId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc]) WHERE ([Status]=(1));


GO

CREATE NONCLUSTERED INDEX [IX_SheetSectionRevisions_SheetVersionId]
    ON [dbo].[SheetSectionRevisions]([SheetVersionId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetSectionRevisions_SheetSectionId_RevisionNumber]
    ON [dbo].[SheetSectionRevisions]([SheetSectionId] ASC, [RevisionNumber] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetSectionRevisions_OneCurrentPerSection]
    ON [dbo].[SheetSectionRevisions]([SheetSectionId] ASC) WHERE ([Status]=(1) AND [SupersededAtUtc] IS NULL);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetSectionRevisions_OneDraftPerSection]
    ON [dbo].[SheetSectionRevisions]([SheetSectionId] ASC) WHERE ([Status]=(0));


GO

CREATE NONCLUSTERED INDEX [IX_SheetSectionRevisions_AuthorUserId_Status]
    ON [dbo].[SheetSectionRevisions]([AuthorUserId] ASC, [Status] ASC);


GO

