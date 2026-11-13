CREATE TABLE [spec].[SheetSectionRevisions] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [SheetSectionId]    INT           NOT NULL,
    [RevisionNumber]    INT           NOT NULL,
    [Status]            INT           NOT NULL,
    [DisplayOrder]      INT           NOT NULL,
    [IsDeleted]         BIT           NOT NULL,
    [AuthorUserId]      INT           NOT NULL,
    [CreatedAtUtc]      DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAtUtc]      DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedAtUtc]    DATETIME2 (7) NULL,
    [SheetVersionId]    INT           NULL,
    [RowVersion]        ROWVERSION    NOT NULL,
    [SupersededAtUtc]   DATETIME2 (7) NULL,
    [SheetId]           INT           NOT NULL,
    [SheetSubmissionId] INT           NULL,
    CONSTRAINT [PK_SheetSectionRevisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetSectionRevisions_PublishedAt] CHECK ([Status]=(0) AND [PublishedAtUtc] IS NULL OR [Status]=(1) AND [PublishedAtUtc] IS NOT NULL),
    CONSTRAINT [CK_SheetSectionRevisions_RevisionNumber] CHECK ([RevisionNumber]>=(1)),
    CONSTRAINT [CK_SheetSectionRevisions_Status] CHECK ([Status]=(1) OR [Status]=(0)),
    CONSTRAINT [CK_SheetSectionRevisions_SupersededAt] CHECK ([SupersededAtUtc] IS NULL OR [Status]=(1) AND [SupersededAtUtc]>=[PublishedAtUtc]),
    CONSTRAINT [FK_SheetSectionRevisions_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]),
    CONSTRAINT [FK_SheetSectionRevisions_SheetSections_SheetSectionId] FOREIGN KEY ([SheetSectionId]) REFERENCES [spec].[SheetSections] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetSectionRevisions_SheetSubmissions_SheetSubmissionId] FOREIGN KEY ([SheetSubmissionId]) REFERENCES [approval].[SheetSubmissions] ([Id]),
    CONSTRAINT [FK_SheetSectionRevisions_SheetVersions_SheetVersionId] FOREIGN KEY ([SheetVersionId]) REFERENCES [spec].[SheetVersions] ([Id]),
    CONSTRAINT [FK_SheetSectionRevisions_Users_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [auth].[Users] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSectionRevisions_SheetPublished]
    ON [spec].[SheetSectionRevisions]([SheetId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc], [IsDeleted], [DisplayOrder], [SheetSectionId]) WHERE ([Status]=(1));


GO

CREATE NONCLUSTERED INDEX [IX_SheetSectionRevisions_Published]
    ON [spec].[SheetSectionRevisions]([SheetSectionId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc]) WHERE ([Status]=(1));


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetSectionRevisions_OneDraftPerSection]
    ON [spec].[SheetSectionRevisions]([SheetSectionId] ASC) WHERE ([Status]=(0));


GO

CREATE NONCLUSTERED INDEX [IX_SheetSectionRevisions_SheetId_Status]
    ON [spec].[SheetSectionRevisions]([SheetId] ASC, [Status] ASC)
    INCLUDE([SupersededAtUtc], [SheetSectionId], [AuthorUserId]);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSectionRevisions_SheetVersionId]
    ON [spec].[SheetSectionRevisions]([SheetVersionId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSectionRevisions_AuthorUserId_Status]
    ON [spec].[SheetSectionRevisions]([AuthorUserId] ASC, [Status] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetSectionRevisions_SheetSectionId_RevisionNumber]
    ON [spec].[SheetSectionRevisions]([SheetSectionId] ASC, [RevisionNumber] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetSectionRevisions_OneCurrentPerSection]
    ON [spec].[SheetSectionRevisions]([SheetSectionId] ASC) WHERE ([Status]=(1) AND [SupersededAtUtc] IS NULL);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSectionRevisions_SheetSubmissionId]
    ON [spec].[SheetSectionRevisions]([SheetSubmissionId] ASC);


GO

