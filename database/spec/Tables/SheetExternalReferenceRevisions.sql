CREATE TABLE [spec].[SheetExternalReferenceRevisions] (
    [Id]                       INT           IDENTITY (1, 1) NOT NULL,
    [SheetExternalReferenceId] INT           NOT NULL,
    [SheetId]                  INT           NOT NULL,
    [SheetVersionId]           INT           NOT NULL,
    [RevisionNumber]           INT           NOT NULL,
    [IsDeleted]                BIT           NOT NULL,
    [PublishedAtUtc]           DATETIME2 (7) NOT NULL,
    [SupersededAtUtc]          DATETIME2 (7) NULL,
    [PushedAtUtc]              DATETIME2 (7) DEFAULT (sysutcdatetime()) NOT NULL,
    [ExternalId]               INT            NOT NULL,
    [SystemName]               NVARCHAR (256) NOT NULL,
    [OwnerName]                NVARCHAR (256) NOT NULL,
    [Url]                      NVARCHAR (MAX) NOT NULL,
    [Type]                     NVARCHAR (32)  NOT NULL,
    [Grouping]                 NVARCHAR (MAX) NULL,
    [SystemIconUrl]            NVARCHAR (MAX) NULL,
    [Description]              NVARCHAR (MAX) NULL,
    [RowVersion]               ROWVERSION    NOT NULL,
    CONSTRAINT [PK_SheetExternalReferenceRevisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetExternalReferenceRevisions_RevisionNumber] CHECK ([RevisionNumber]>=(1)),
    CONSTRAINT [CK_SheetExternalReferenceRevisions_SupersededAt] CHECK ([SupersededAtUtc] IS NULL OR [SupersededAtUtc]>=[PublishedAtUtc]),
    CONSTRAINT [FK_SheetExternalReferenceRevisions_SheetExternalReferences_SheetExternalReferenceId] FOREIGN KEY ([SheetExternalReferenceId]) REFERENCES [spec].[SheetExternalReferences] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetExternalReferenceRevisions_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]),
    CONSTRAINT [FK_SheetExternalReferenceRevisions_SheetVersions_SheetVersionId] FOREIGN KEY ([SheetVersionId]) REFERENCES [spec].[SheetVersions] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetExternalReferenceRevisions_Published]
    ON [spec].[SheetExternalReferenceRevisions]([SheetExternalReferenceId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc]);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetExternalReferenceRevisions_SheetExternalReferenceId_RevisionNumber]
    ON [spec].[SheetExternalReferenceRevisions]([SheetExternalReferenceId] ASC, [RevisionNumber] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetExternalReferenceRevisions_SheetPublished]
    ON [spec].[SheetExternalReferenceRevisions]([SheetId] ASC, [PublishedAtUtc] ASC)
    INCLUDE([SupersededAtUtc], [IsDeleted], [SheetExternalReferenceId]);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetExternalReferenceRevisions_OneCurrentPerReference]
    ON [spec].[SheetExternalReferenceRevisions]([SheetExternalReferenceId] ASC) WHERE ([SupersededAtUtc] IS NULL);


GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetExternalReferenceRevisions_OnePerVersion]
    ON [spec].[SheetExternalReferenceRevisions]([SheetExternalReferenceId] ASC, [SheetVersionId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetExternalReferenceRevisions_SheetVersionId]
    ON [spec].[SheetExternalReferenceRevisions]([SheetVersionId] ASC);


GO

