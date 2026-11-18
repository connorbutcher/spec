CREATE TABLE [spec].[SheetTableRevisions] (
    [SheetTableRevisionID]  INT            IDENTITY (1, 1) NOT NULL,
    [SheetTableID]          INT            NOT NULL,
    [SheetID]               INT            NOT NULL,
    [RevisionNumber]        INT            NOT NULL,
    [Status]                TINYINT        NOT NULL,
    [Title]                 VARCHAR (64)   NOT NULL,
    [DisplayOrder]          INT            NOT NULL,
    [ConfigurationOverride] NVARCHAR (MAX) NULL,
    [IsDeleted]             BIT            CONSTRAINT [DF_SheetTableRevisions_IsDeleted] DEFAULT ((0)) NOT NULL,
    [CreatedByUserID]       SMALLINT       NOT NULL,
    [CreatedDateTime]       DATETIME2 (7)  CONSTRAINT [DF_SheetTableRevisions_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [LastModifiedDateTime]  DATETIME2 (7)  CONSTRAINT [DF_SheetTableRevisions_LastModifiedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedDateTime]     DATETIME2 (7)  NULL,
    [SupersededDateTime]    DATETIME2 (7)  NULL,
    [SheetVersionID]        INT            NULL,
    [SheetSubmissionID]     INT            NULL,
    [RowVersion]            ROWVERSION     NOT NULL,
    CONSTRAINT [PK_SheetTableRevisions] PRIMARY KEY CLUSTERED ([SheetTableRevisionID] ASC),
    CONSTRAINT [CK_SheetTableRevisions_RevisionNumber] CHECK ([RevisionNumber]>=(1)),
    CONSTRAINT [CK_SheetTableRevisions_Status] CHECK ([Status]=(0) OR [Status]=(1)),
    CONSTRAINT [CK_SheetTableRevisions_PublishedDateTime] CHECK ([Status]=(0) AND [PublishedDateTime] IS NULL OR [Status]=(1) AND [PublishedDateTime] IS NOT NULL),
    CONSTRAINT [CK_SheetTableRevisions_SupersededDateTime] CHECK ([SupersededDateTime] IS NULL OR [Status]=(1) AND [SupersededDateTime]>=[PublishedDateTime]),
    CONSTRAINT [CK_SheetTableRevisions_ConfigurationOverride] CHECK ([ConfigurationOverride] IS NULL OR isjson([ConfigurationOverride])=(1)),
    CONSTRAINT [FK_SheetTableRevisions_SheetTables_SheetTableID] FOREIGN KEY ([SheetTableID]) REFERENCES [spec].[SheetTables] ([SheetTableID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableRevisions_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]),
    CONSTRAINT [FK_SheetTableRevisions_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SheetTableRevisions_SheetVersions_SheetVersionID] FOREIGN KEY ([SheetVersionID]) REFERENCES [spec].[SheetVersions] ([SheetVersionID]),
    CONSTRAINT [FK_SheetTableRevisions_SheetSubmissions_SheetSubmissionID] FOREIGN KEY ([SheetSubmissionID]) REFERENCES [approval].[SheetSubmissions] ([SheetSubmissionID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableRevisions_SheetTableID_RevisionNumber]
    ON [spec].[SheetTableRevisions]([SheetTableID] ASC, [RevisionNumber] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableRevisions_OneDraft]
    ON [spec].[SheetTableRevisions]([SheetTableID] ASC) WHERE ([Status]=(0));

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableRevisions_OneCurrent]
    ON [spec].[SheetTableRevisions]([SheetTableID] ASC) WHERE ([Status]=(1) AND [SupersededDateTime] IS NULL);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRevisions_Published]
    ON [spec].[SheetTableRevisions]([SheetTableID] ASC, [PublishedDateTime] ASC)
    INCLUDE([SupersededDateTime]) WHERE ([Status]=(1));

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRevisions_SheetPublished]
    ON [spec].[SheetTableRevisions]([SheetID] ASC, [PublishedDateTime] ASC)
    INCLUDE([SupersededDateTime], [IsDeleted], [DisplayOrder], [SheetTableID]) WHERE ([Status]=(1));

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRevisions_SheetID_Status]
    ON [spec].[SheetTableRevisions]([SheetID] ASC, [Status] ASC)
    INCLUDE([SupersededDateTime], [SheetTableID], [CreatedByUserID]);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRevisions_CreatedByUserID_Status]
    ON [spec].[SheetTableRevisions]([CreatedByUserID] ASC, [Status] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRevisions_SheetVersionID]
    ON [spec].[SheetTableRevisions]([SheetVersionID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRevisions_SheetSubmissionID]
    ON [spec].[SheetTableRevisions]([SheetSubmissionID] ASC);

