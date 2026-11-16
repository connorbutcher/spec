CREATE TABLE [spec].[SheetPropertyRevisions] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [SheetPropertyId]   INT           NOT NULL,
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
    CONSTRAINT [PK_SheetPropertyRevisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetPropertyRevisions_PublishedAt] CHECK ([Status]=(0) AND [PublishedAtUtc] IS NULL OR [Status]=(1) AND [PublishedAtUtc] IS NOT NULL),
    CONSTRAINT [CK_SheetPropertyRevisions_RevisionNumber] CHECK ([RevisionNumber]>=(1)),
    CONSTRAINT [CK_SheetPropertyRevisions_Status] CHECK ([Status]=(1) OR [Status]=(0)),
    CONSTRAINT [CK_SheetPropertyRevisions_SupersededAt] CHECK ([SupersededAtUtc] IS NULL OR [Status]=(1) AND [SupersededAtUtc]>=[PublishedAtUtc]),
    CONSTRAINT [CK_SheetPropertyRevisions_Version] CHECK ([Status]=(0) AND [SheetVersionId] IS NULL OR [Status]=(1) AND [SheetVersionId] IS NOT NULL),
    CONSTRAINT [FK_SheetPropertyRevisions_SheetProperties_SheetPropertyId] FOREIGN KEY ([SheetPropertyId]) REFERENCES [spec].[SheetProperties] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetPropertyRevisions_SheetSubmissions_SheetSubmissionId] FOREIGN KEY ([SheetSubmissionId]) REFERENCES [approval].[SheetSubmissions] ([Id]),
    CONSTRAINT [FK_SheetPropertyRevisions_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]),
    CONSTRAINT [FK_SheetPropertyRevisions_SheetVersions_SheetVersionId] FOREIGN KEY ([SheetVersionId]) REFERENCES [spec].[SheetVersions] ([Id]),
    CONSTRAINT [FK_SheetPropertyRevisions_Users_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [auth].[Users] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_Published]
    ON [spec].[SheetPropertyRevisions]([SheetPropertyId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc]) WHERE ([Status]=(1));


GO

CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_AuthorUserId_Status]
    ON [spec].[SheetPropertyRevisions]([AuthorUserId] ASC, [Status] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_SheetPropertyId_RevisionNumber]
    ON [spec].[SheetPropertyRevisions]([SheetPropertyId] ASC, [RevisionNumber] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_SheetId_Status]
    ON [spec].[SheetPropertyRevisions]([SheetId] ASC, [Status] ASC)
    INCLUDE([SupersededAtUtc], [SheetPropertyId], [AuthorUserId]);


GO

CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_SheetPublished]
    ON [spec].[SheetPropertyRevisions]([SheetId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc], [IsDeleted], [DisplayOrder], [SheetPropertyId]) WHERE ([Status]=(1));


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetPropertyRevisions_OneCurrentPerProperty]
    ON [spec].[SheetPropertyRevisions]([SheetPropertyId] ASC) WHERE ([Status]=(1) AND [SupersededAtUtc] IS NULL);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetPropertyRevisions_OneDraftPerProperty]
    ON [spec].[SheetPropertyRevisions]([SheetPropertyId] ASC) WHERE ([Status]=(0));


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetPropertyRevisions_OnePerVersion]
    ON [spec].[SheetPropertyRevisions]([SheetPropertyId] ASC, [SheetVersionId] ASC) WHERE ([SheetVersionId] IS NOT NULL);


GO

CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_SheetVersionId]
    ON [spec].[SheetPropertyRevisions]([SheetVersionId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_SheetSubmissionId]
    ON [spec].[SheetPropertyRevisions]([SheetSubmissionId] ASC);


GO

