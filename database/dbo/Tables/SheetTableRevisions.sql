CREATE TABLE [dbo].[SheetTableRevisions] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [SheetTableId]    INT            NOT NULL,
    [RevisionNumber]  INT            NOT NULL,
    [Status]          INT            NOT NULL,
    [Title]           NVARCHAR (200) NULL,
    [IsDeleted]       BIT            NOT NULL,
    [AuthorUserId]    INT            NOT NULL,
    [CreatedAtUtc]    DATETIME2 (7)  DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAtUtc]    DATETIME2 (7)  DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedAtUtc]  DATETIME2 (7)  NULL,
    [SheetVersionId]  INT            NULL,
    [DisplayOrder]    INT            DEFAULT ((0)) NOT NULL,
    [RowVersion]      ROWVERSION     NOT NULL,
    [SupersededAtUtc] DATETIME2 (7)  NULL,
    CONSTRAINT [PK_SheetTableRevisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetTableRevisions_PublishedAt] CHECK ([Status]=(0) AND [PublishedAtUtc] IS NULL OR [Status]=(1) AND [PublishedAtUtc] IS NOT NULL),
    CONSTRAINT [CK_SheetTableRevisions_SupersededAt] CHECK ([SupersededAtUtc] IS NULL OR [Status]=(1) AND [SupersededAtUtc]>=[PublishedAtUtc]),
    CONSTRAINT [FK_SheetTableRevisions_SheetTables_SheetTableId] FOREIGN KEY ([SheetTableId]) REFERENCES [dbo].[SheetTables] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableRevisions_SheetVersions_SheetVersionId] FOREIGN KEY ([SheetVersionId]) REFERENCES [dbo].[SheetVersions] ([Id]),
    CONSTRAINT [FK_SheetTableRevisions_Users_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [dbo].[Users] ([Id])
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableRevisions_OneDraftPerTable]
    ON [dbo].[SheetTableRevisions]([SheetTableId] ASC) WHERE ([Status]=(0));


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetTableRevisions_SheetTableId_RevisionNumber]
    ON [dbo].[SheetTableRevisions]([SheetTableId] ASC, [RevisionNumber] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetTableRevisions_Published]
    ON [dbo].[SheetTableRevisions]([SheetTableId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc]) WHERE ([Status]=(1));


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableRevisions_OneCurrentPerTable]
    ON [dbo].[SheetTableRevisions]([SheetTableId] ASC) WHERE ([Status]=(1) AND [SupersededAtUtc] IS NULL);


GO

CREATE NONCLUSTERED INDEX [IX_SheetTableRevisions_SheetVersionId]
    ON [dbo].[SheetTableRevisions]([SheetVersionId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetTableRevisions_AuthorUserId_Status]
    ON [dbo].[SheetTableRevisions]([AuthorUserId] ASC, [Status] ASC);


GO

