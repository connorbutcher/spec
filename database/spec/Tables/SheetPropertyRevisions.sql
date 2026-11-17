CREATE TABLE [spec].[SheetPropertyRevisions] (
    [SheetPropertyRevisionID] INT            IDENTITY (1, 1) NOT NULL,
    [SheetPropertyID]         INT            NOT NULL,
    [SheetID]                 INT            NOT NULL,
    [RevisionNumber]          INT            NOT NULL,
    [Status]                  TINYINT        NOT NULL,
    [PropertyValue]           NVARCHAR (MAX) NULL,
    [IsDeleted]               BIT            CONSTRAINT [DF_SheetPropertyRevisions_IsDeleted] DEFAULT ((0)) NOT NULL,
    [CreatedByUserID]         SMALLINT       NOT NULL,
    [CreatedDateTime]         DATETIME2 (7)  CONSTRAINT [DF_SheetPropertyRevisions_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [LastModifiedDateTime]    DATETIME2 (7)  CONSTRAINT [DF_SheetPropertyRevisions_LastModifiedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedDateTime]       DATETIME2 (7)  NULL,
    [SupersededDateTime]      DATETIME2 (7)  NULL,
    [SheetVersionID]          INT            NULL,
    [SheetSubmissionID]       INT            NULL,
    [RowVersion]              ROWVERSION     NOT NULL,
    CONSTRAINT [PK_SheetPropertyRevisions] PRIMARY KEY CLUSTERED ([SheetPropertyRevisionID] ASC),
    CONSTRAINT [CK_SheetPropertyRevisions_RevisionNumber] CHECK ([RevisionNumber]>=(1)),
    CONSTRAINT [CK_SheetPropertyRevisions_Status] CHECK ([Status]=(0) OR [Status]=(1)),
    CONSTRAINT [CK_SheetPropertyRevisions_PublishedDateTime] CHECK ([Status]=(0) AND [PublishedDateTime] IS NULL OR [Status]=(1) AND [PublishedDateTime] IS NOT NULL),
    CONSTRAINT [CK_SheetPropertyRevisions_SupersededDateTime] CHECK ([SupersededDateTime] IS NULL OR [Status]=(1) AND [SupersededDateTime]>=[PublishedDateTime]),
    CONSTRAINT [FK_SheetPropertyRevisions_SheetProperties_SheetPropertyID] FOREIGN KEY ([SheetPropertyID]) REFERENCES [spec].[SheetProperties] ([SheetPropertyID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetPropertyRevisions_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]),
    CONSTRAINT [FK_SheetPropertyRevisions_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SheetPropertyRevisions_SheetVersions_SheetVersionID] FOREIGN KEY ([SheetVersionID]) REFERENCES [spec].[SheetVersions] ([SheetVersionID]),
    CONSTRAINT [FK_SheetPropertyRevisions_SheetSubmissions_SheetSubmissionID] FOREIGN KEY ([SheetSubmissionID]) REFERENCES [approval].[SheetSubmissions] ([SheetSubmissionID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetPropertyRevisions_SheetPropertyID_RevisionNumber]
    ON [spec].[SheetPropertyRevisions]([SheetPropertyID] ASC, [RevisionNumber] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetPropertyRevisions_OneDraft]
    ON [spec].[SheetPropertyRevisions]([SheetPropertyID] ASC) WHERE ([Status]=(0));

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetPropertyRevisions_OneCurrent]
    ON [spec].[SheetPropertyRevisions]([SheetPropertyID] ASC) WHERE ([Status]=(1) AND [SupersededDateTime] IS NULL);

GO
CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_Published]
    ON [spec].[SheetPropertyRevisions]([SheetPropertyID] ASC, [PublishedDateTime] ASC)
    INCLUDE([SupersededDateTime]) WHERE ([Status]=(1));

GO
CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_SheetPublished]
    ON [spec].[SheetPropertyRevisions]([SheetID] ASC, [PublishedDateTime] ASC)
    INCLUDE([SupersededDateTime], [IsDeleted], [SheetPropertyID]) WHERE ([Status]=(1));

GO
CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_SheetID_Status]
    ON [spec].[SheetPropertyRevisions]([SheetID] ASC, [Status] ASC)
    INCLUDE([SupersededDateTime], [SheetPropertyID], [CreatedByUserID]);

GO
CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_CreatedByUserID_Status]
    ON [spec].[SheetPropertyRevisions]([CreatedByUserID] ASC, [Status] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_SheetVersionID]
    ON [spec].[SheetPropertyRevisions]([SheetVersionID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetPropertyRevisions_SheetSubmissionID]
    ON [spec].[SheetPropertyRevisions]([SheetSubmissionID] ASC);

