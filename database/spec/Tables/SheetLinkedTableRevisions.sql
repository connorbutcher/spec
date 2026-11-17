CREATE TABLE [spec].[SheetLinkedTableRevisions] (
    [SheetLinkedTableRevisionID] INT           IDENTITY (1, 1) NOT NULL,
    [SheetLinkedTableID]         INT           NOT NULL,
    [SheetID]                    INT           NOT NULL,
    [RevisionNumber]             INT           NOT NULL,
    [Status]                     TINYINT       NOT NULL,
    [DisplayOrder]               INT           NOT NULL,
    [IsDeleted]                  BIT           CONSTRAINT [DF_SheetLinkedTableRevisions_IsDeleted] DEFAULT ((0)) NOT NULL,
    [CreatedByUserID]            SMALLINT      NOT NULL,
    [CreatedDateTime]            DATETIME2 (7) CONSTRAINT [DF_SheetLinkedTableRevisions_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [LastModifiedDateTime]       DATETIME2 (7) CONSTRAINT [DF_SheetLinkedTableRevisions_LastModifiedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedDateTime]          DATETIME2 (7) NULL,
    [SupersededDateTime]         DATETIME2 (7) NULL,
    [SheetVersionID]             INT           NULL,
    [SheetSubmissionID]          INT           NULL,
    [RowVersion]                 ROWVERSION    NOT NULL,
    CONSTRAINT [PK_SheetLinkedTableRevisions] PRIMARY KEY CLUSTERED ([SheetLinkedTableRevisionID] ASC),
    CONSTRAINT [CK_SheetLinkedTableRevisions_RevisionNumber] CHECK ([RevisionNumber]>=(1)),
    CONSTRAINT [CK_SheetLinkedTableRevisions_Status] CHECK ([Status]=(0) OR [Status]=(1)),
    CONSTRAINT [CK_SheetLinkedTableRevisions_PublishedDateTime] CHECK ([Status]=(0) AND [PublishedDateTime] IS NULL OR [Status]=(1) AND [PublishedDateTime] IS NOT NULL),
    CONSTRAINT [CK_SheetLinkedTableRevisions_SupersededDateTime] CHECK ([SupersededDateTime] IS NULL OR [Status]=(1) AND [SupersededDateTime]>=[PublishedDateTime]),
    CONSTRAINT [FK_SheetLinkedTableRevisions_SheetLinkedTables_SheetLinkedTableID] FOREIGN KEY ([SheetLinkedTableID]) REFERENCES [spec].[SheetLinkedTables] ([SheetLinkedTableID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetLinkedTableRevisions_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]),
    CONSTRAINT [FK_SheetLinkedTableRevisions_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SheetLinkedTableRevisions_SheetVersions_SheetVersionID] FOREIGN KEY ([SheetVersionID]) REFERENCES [spec].[SheetVersions] ([SheetVersionID]),
    CONSTRAINT [FK_SheetLinkedTableRevisions_SheetSubmissions_SheetSubmissionID] FOREIGN KEY ([SheetSubmissionID]) REFERENCES [approval].[SheetSubmissions] ([SheetSubmissionID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetLinkedTableRevisions_SheetLinkedTableID_RevisionNumber]
    ON [spec].[SheetLinkedTableRevisions]([SheetLinkedTableID] ASC, [RevisionNumber] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetLinkedTableRevisions_OneDraft]
    ON [spec].[SheetLinkedTableRevisions]([SheetLinkedTableID] ASC) WHERE ([Status]=(0));

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetLinkedTableRevisions_OneCurrent]
    ON [spec].[SheetLinkedTableRevisions]([SheetLinkedTableID] ASC) WHERE ([Status]=(1) AND [SupersededDateTime] IS NULL);

GO
CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_Published]
    ON [spec].[SheetLinkedTableRevisions]([SheetLinkedTableID] ASC, [PublishedDateTime] ASC)
    INCLUDE([SupersededDateTime]) WHERE ([Status]=(1));

GO
CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_SheetPublished]
    ON [spec].[SheetLinkedTableRevisions]([SheetID] ASC, [PublishedDateTime] ASC)
    INCLUDE([SupersededDateTime], [IsDeleted], [DisplayOrder], [SheetLinkedTableID]) WHERE ([Status]=(1));

GO
CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_SheetID_Status]
    ON [spec].[SheetLinkedTableRevisions]([SheetID] ASC, [Status] ASC)
    INCLUDE([SupersededDateTime], [SheetLinkedTableID], [CreatedByUserID]);

GO
CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_CreatedByUserID_Status]
    ON [spec].[SheetLinkedTableRevisions]([CreatedByUserID] ASC, [Status] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_SheetVersionID]
    ON [spec].[SheetLinkedTableRevisions]([SheetVersionID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetLinkedTableRevisions_SheetSubmissionID]
    ON [spec].[SheetLinkedTableRevisions]([SheetSubmissionID] ASC);

