CREATE TABLE [spec].[SheetLinkedTableRevisions] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [SheetLinkedTableId] INT           NOT NULL,
    [RevisionNumber]    INT           NOT NULL,
    [Status]            INT           NOT NULL,
    [DisplayOrder]      INT           NOT NULL,
    [IsDeleted]         BIT           NOT NULL,
    [AuthorUserId]      INT           NOT NULL,
    [CreatedAtUtc]      DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAtUtc]      DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedAtUtc]    DATETIME2 (7) NULL,
    [SupersededAtUtc]   DATETIME2 (7) NULL,
    [RowVersion]        ROWVERSION    NOT NULL,
    [SheetVersionId]    INT           NULL,
    [SheetId]           INT           NOT NULL,
    [SheetSubmissionId] INT           NULL,
    CONSTRAINT [PK_SheetLinkedTableRevisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetLinkedTableRevisions_PublishedAt] CHECK ([Status]=(0) AND [PublishedAtUtc] IS NULL OR [Status]=(1) AND [PublishedAtUtc] IS NOT NULL),
    CONSTRAINT [CK_SheetLinkedTableRevisions_RevisionNumber] CHECK ([RevisionNumber]>=(1)),
    CONSTRAINT [CK_SheetLinkedTableRevisions_Status] CHECK ([Status]=(1) OR [Status]=(0)),
    CONSTRAINT [CK_SheetLinkedTableRevisions_SupersededAt] CHECK ([SupersededAtUtc] IS NULL OR [Status]=(1) AND [SupersededAtUtc]>=[PublishedAtUtc]),
    CONSTRAINT [FK_SheetLinkedTableRevisions_SheetLinkedTables_SheetLinkedTableId] FOREIGN KEY ([SheetLinkedTableId]) REFERENCES [spec].[SheetLinkedTables] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetLinkedTableRevisions_SheetSubmissions_SheetSubmissionId] FOREIGN KEY ([SheetSubmissionId]) REFERENCES [approval].[SheetSubmissions] ([Id]),
    CONSTRAINT [FK_SheetLinkedTableRevisions_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]),
    CONSTRAINT [FK_SheetLinkedTableRevisions_SheetVersions_SheetVersionId] FOREIGN KEY ([SheetVersionId]) REFERENCES [spec].[SheetVersions] ([Id]),
    CONSTRAINT [FK_SheetLinkedTableRevisions_Users_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [auth].[Users] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_Published]
    ON [spec].[SheetLinkedTableRevisions]([SheetLinkedTableId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc]) WHERE ([Status]=(1));


GO

CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_AuthorUserId_Status]
    ON [spec].[SheetLinkedTableRevisions]([AuthorUserId] ASC, [Status] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_SheetLinkedTableId_RevisionNumber]
    ON [spec].[SheetLinkedTableRevisions]([SheetLinkedTableId] ASC, [RevisionNumber] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_SheetId_Status]
    ON [spec].[SheetLinkedTableRevisions]([SheetId] ASC, [Status] ASC)
    INCLUDE([SupersededAtUtc], [SheetLinkedTableId], [AuthorUserId]);


GO

CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_SheetPublished]
    ON [spec].[SheetLinkedTableRevisions]([SheetId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc], [IsDeleted], [DisplayOrder], [SheetLinkedTableId]) WHERE ([Status]=(1));


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetLinkedTableRevisions_OneCurrentPerLinkedTable]
    ON [spec].[SheetLinkedTableRevisions]([SheetLinkedTableId] ASC) WHERE ([Status]=(1) AND [SupersededAtUtc] IS NULL);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetLinkedTableRevisions_OneDraftPerLinkedTable]
    ON [spec].[SheetLinkedTableRevisions]([SheetLinkedTableId] ASC) WHERE ([Status]=(0));


GO

CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_SheetVersionId]
    ON [spec].[SheetLinkedTableRevisions]([SheetVersionId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_SheetSubmissionId]
    ON [spec].[SheetLinkedTableRevisions]([SheetSubmissionId] ASC);


GO

