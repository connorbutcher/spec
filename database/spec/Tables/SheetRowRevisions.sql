CREATE TABLE [spec].[SheetRowRevisions] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [SheetRowId]        INT           NOT NULL,
    [RevisionNumber]    INT           NOT NULL,
    [Status]            INT           NOT NULL,
    [IsDeleted]         BIT           NOT NULL,
    [AuthorUserId]      INT           NOT NULL,
    [CreatedAtUtc]      DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAtUtc]      DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedAtUtc]    DATETIME2 (7) NULL,
    [SheetVersionId]    INT           NULL,
    [DisplayOrder]      INT           DEFAULT ((0)) NOT NULL,
    [RowVersion]        ROWVERSION    NOT NULL,
    [SupersededAtUtc]   DATETIME2 (7) NULL,
    [SheetId]           INT           NOT NULL,
    [SheetSubmissionId] INT           NULL,
    CONSTRAINT [PK_SheetRowRevisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetRowRevisions_PublishedAt] CHECK ([Status]=(0) AND [PublishedAtUtc] IS NULL OR [Status]=(1) AND [PublishedAtUtc] IS NOT NULL),
    CONSTRAINT [CK_SheetRowRevisions_RevisionNumber] CHECK ([RevisionNumber]>=(1)),
    CONSTRAINT [CK_SheetRowRevisions_Status] CHECK ([Status]=(1) OR [Status]=(0)),
    CONSTRAINT [CK_SheetRowRevisions_SupersededAt] CHECK ([SupersededAtUtc] IS NULL OR [Status]=(1) AND [SupersededAtUtc]>=[PublishedAtUtc]),
    CONSTRAINT [FK_SheetRowRevisions_SheetRows_SheetRowId] FOREIGN KEY ([SheetRowId]) REFERENCES [spec].[SheetRows] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetRowRevisions_SheetSubmissions_SheetSubmissionId] FOREIGN KEY ([SheetSubmissionId]) REFERENCES [approval].[SheetSubmissions] ([Id]),
    CONSTRAINT [FK_SheetRowRevisions_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]),
    CONSTRAINT [FK_SheetRowRevisions_SheetVersions_SheetVersionId] FOREIGN KEY ([SheetVersionId]) REFERENCES [spec].[SheetVersions] ([Id]),
    CONSTRAINT [FK_SheetRowRevisions_Users_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [auth].[Users] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetRowRevisions_Published]
    ON [spec].[SheetRowRevisions]([SheetRowId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc]) WHERE ([Status]=(1));


GO

CREATE NONCLUSTERED INDEX [IX_SheetRowRevisions_SheetId_Status]
    ON [spec].[SheetRowRevisions]([SheetId] ASC, [Status] ASC)
    INCLUDE([SupersededAtUtc], [SheetRowId], [AuthorUserId]);


GO

CREATE NONCLUSTERED INDEX [IX_SheetRowRevisions_SheetPublished]
    ON [spec].[SheetRowRevisions]([SheetId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc], [IsDeleted], [DisplayOrder], [SheetRowId]) WHERE ([Status]=(1));


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetRowRevisions_OneDraftPerRow]
    ON [spec].[SheetRowRevisions]([SheetRowId] ASC) WHERE ([Status]=(0));


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetRowRevisions_OneCurrentPerRow]
    ON [spec].[SheetRowRevisions]([SheetRowId] ASC) WHERE ([Status]=(1) AND [SupersededAtUtc] IS NULL);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetRowRevisions_SheetRowId_RevisionNumber]
    ON [spec].[SheetRowRevisions]([SheetRowId] ASC, [RevisionNumber] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetRowRevisions_SheetVersionId]
    ON [spec].[SheetRowRevisions]([SheetVersionId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetRowRevisions_AuthorUserId_Status]
    ON [spec].[SheetRowRevisions]([AuthorUserId] ASC, [Status] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetRowRevisions_SheetSubmissionId]
    ON [spec].[SheetRowRevisions]([SheetSubmissionId] ASC);


GO

