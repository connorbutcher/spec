CREATE TABLE [dbo].[SheetRowRevisions] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [SheetRowId]      INT           NOT NULL,
    [RevisionNumber]  INT           NOT NULL,
    [Status]          INT           NOT NULL,
    [IsDeleted]       BIT           NOT NULL,
    [AuthorUserId]    INT           NOT NULL,
    [CreatedAtUtc]    DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAtUtc]    DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedAtUtc]  DATETIME2 (7) NULL,
    [SheetVersionId]  INT           NULL,
    [DisplayOrder]    INT           DEFAULT ((0)) NOT NULL,
    [RowVersion]      ROWVERSION    NOT NULL,
    [SupersededAtUtc] DATETIME2 (7) NULL,
    CONSTRAINT [PK_SheetRowRevisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetRowRevisions_PublishedAt] CHECK ([Status]=(0) AND [PublishedAtUtc] IS NULL OR [Status]=(1) AND [PublishedAtUtc] IS NOT NULL),
    CONSTRAINT [CK_SheetRowRevisions_SupersededAt] CHECK ([SupersededAtUtc] IS NULL OR [Status]=(1) AND [SupersededAtUtc]>=[PublishedAtUtc]),
    CONSTRAINT [FK_SheetRowRevisions_SheetRows_SheetRowId] FOREIGN KEY ([SheetRowId]) REFERENCES [dbo].[SheetRows] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetRowRevisions_SheetVersions_SheetVersionId] FOREIGN KEY ([SheetVersionId]) REFERENCES [dbo].[SheetVersions] ([Id]),
    CONSTRAINT [FK_SheetRowRevisions_Users_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [dbo].[Users] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetRowRevisions_AuthorUserId_Status]
    ON [dbo].[SheetRowRevisions]([AuthorUserId] ASC, [Status] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetRowRevisions_OneDraftPerRow]
    ON [dbo].[SheetRowRevisions]([SheetRowId] ASC) WHERE ([Status]=(0));


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetRowRevisions_OneCurrentPerRow]
    ON [dbo].[SheetRowRevisions]([SheetRowId] ASC) WHERE ([Status]=(1) AND [SupersededAtUtc] IS NULL);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetRowRevisions_SheetRowId_RevisionNumber]
    ON [dbo].[SheetRowRevisions]([SheetRowId] ASC, [RevisionNumber] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetRowRevisions_Published]
    ON [dbo].[SheetRowRevisions]([SheetRowId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc]) WHERE ([Status]=(1));


GO

CREATE NONCLUSTERED INDEX [IX_SheetRowRevisions_SheetVersionId]
    ON [dbo].[SheetRowRevisions]([SheetVersionId] ASC);


GO

