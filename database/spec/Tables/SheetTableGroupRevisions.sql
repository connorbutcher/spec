CREATE TABLE [spec].[SheetTableGroupRevisions] (
    [SheetTableGroupRevisionID]        INT              IDENTITY (1, 1) NOT NULL,
    [SheetTableGroupID]                INT              NOT NULL,
    [SheetID]                          INT              NOT NULL,
    [RevisionNumber]                   INT              NOT NULL,
    [Status]                           TINYINT          NOT NULL,
    [DisplayOrder]                     INT              NOT NULL,
    [LinkedSheetTableGroupReferenceID] UNIQUEIDENTIFIER NULL,
    [IsDeleted]                        BIT              CONSTRAINT [DF_SheetTableGroupRevisions_IsDeleted] DEFAULT ((0)) NOT NULL,
    [CreatedByUserID]                  SMALLINT         NOT NULL,
    [CreatedDateTime]                  DATETIME2 (7)    CONSTRAINT [DF_SheetTableGroupRevisions_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [LastModifiedDateTime]             DATETIME2 (7)    CONSTRAINT [DF_SheetTableGroupRevisions_LastModifiedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedDateTime]                DATETIME2 (7)    NULL,
    [SupersededDateTime]               DATETIME2 (7)    NULL,
    [SheetVersionID]                   INT              NULL,
    [SheetSubmissionID]                INT              NULL,
    [RowVersion]                       ROWVERSION       NOT NULL,
    CONSTRAINT [PK_SheetTableGroupRevisions] PRIMARY KEY CLUSTERED ([SheetTableGroupRevisionID] ASC),
    CONSTRAINT [CK_SheetTableGroupRevisions_RevisionNumber] CHECK ([RevisionNumber]>=(1)),
    CONSTRAINT [CK_SheetTableGroupRevisions_Status] CHECK ([Status]=(0) OR [Status]=(1)),
    CONSTRAINT [CK_SheetTableGroupRevisions_PublishedDateTime] CHECK ([Status]=(0) AND [PublishedDateTime] IS NULL OR [Status]=(1) AND [PublishedDateTime] IS NOT NULL),
    CONSTRAINT [CK_SheetTableGroupRevisions_SupersededDateTime] CHECK ([SupersededDateTime] IS NULL OR [Status]=(1) AND [SupersededDateTime]>=[PublishedDateTime]),
    CONSTRAINT [FK_SheetTableGroupRevisions_SheetTableGroups_SheetTableGroupID] FOREIGN KEY ([SheetTableGroupID]) REFERENCES [spec].[SheetTableGroups] ([SheetTableGroupID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableGroupRevisions_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]),
    CONSTRAINT [FK_SheetTableGroupRevisions_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SheetTableGroupRevisions_SheetVersions_SheetVersionID] FOREIGN KEY ([SheetVersionID]) REFERENCES [spec].[SheetVersions] ([SheetVersionID]),
    CONSTRAINT [FK_SheetTableGroupRevisions_SheetSubmissions_SheetSubmissionID] FOREIGN KEY ([SheetSubmissionID]) REFERENCES [approval].[SheetSubmissions] ([SheetSubmissionID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableGroupRevisions_SheetTableGroupID_RevisionNumber]
    ON [spec].[SheetTableGroupRevisions]([SheetTableGroupID] ASC, [RevisionNumber] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableGroupRevisions_OneDraft]
    ON [spec].[SheetTableGroupRevisions]([SheetTableGroupID] ASC) WHERE ([Status]=(0));

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableGroupRevisions_OneCurrent]
    ON [spec].[SheetTableGroupRevisions]([SheetTableGroupID] ASC) WHERE ([Status]=(1) AND [SupersededDateTime] IS NULL);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableGroupRevisions_Published]
    ON [spec].[SheetTableGroupRevisions]([SheetTableGroupID] ASC, [PublishedDateTime] ASC)
    INCLUDE([SupersededDateTime]) WHERE ([Status]=(1));

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableGroupRevisions_SheetPublished]
    ON [spec].[SheetTableGroupRevisions]([SheetID] ASC, [PublishedDateTime] ASC)
    INCLUDE([SupersededDateTime], [IsDeleted], [DisplayOrder], [SheetTableGroupID]) WHERE ([Status]=(1));

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableGroupRevisions_SheetID_Status]
    ON [spec].[SheetTableGroupRevisions]([SheetID] ASC, [Status] ASC)
    INCLUDE([SupersededDateTime], [SheetTableGroupID], [CreatedByUserID]);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableGroupRevisions_CreatedByUserID_Status]
    ON [spec].[SheetTableGroupRevisions]([CreatedByUserID] ASC, [Status] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableGroupRevisions_SheetVersionID]
    ON [spec].[SheetTableGroupRevisions]([SheetVersionID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableGroupRevisions_SheetSubmissionID]
    ON [spec].[SheetTableGroupRevisions]([SheetSubmissionID] ASC);

