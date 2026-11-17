CREATE TABLE [spec].[SheetTableRowRevisions] (
    [SheetTableRowRevisionID] INT           IDENTITY (1, 1) NOT NULL,
    [SheetTableRowID]         INT           NOT NULL,
    [SheetID]                 INT           NOT NULL,
    [RevisionNumber]          INT           NOT NULL,
    [Status]                  TINYINT       NOT NULL,
    [DisplayOrder]            INT           NOT NULL,
    [IsDeleted]               BIT           CONSTRAINT [DF_SheetTableRowRevisions_IsDeleted] DEFAULT ((0)) NOT NULL,
    [CreatedByUserID]         SMALLINT      NOT NULL,
    [CreatedDateTime]         DATETIME2 (7) CONSTRAINT [DF_SheetTableRowRevisions_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [LastModifiedDateTime]    DATETIME2 (7) CONSTRAINT [DF_SheetTableRowRevisions_LastModifiedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedDateTime]       DATETIME2 (7) NULL,
    [SupersededDateTime]      DATETIME2 (7) NULL,
    [SheetVersionID]          INT           NULL,
    [SheetSubmissionID]       INT           NULL,
    [RowVersion]              ROWVERSION    NOT NULL,
    CONSTRAINT [PK_SheetTableRowRevisions] PRIMARY KEY CLUSTERED ([SheetTableRowRevisionID] ASC),
    CONSTRAINT [CK_SheetTableRowRevisions_RevisionNumber] CHECK ([RevisionNumber]>=(1)),
    CONSTRAINT [CK_SheetTableRowRevisions_Status] CHECK ([Status]=(0) OR [Status]=(1)),
    CONSTRAINT [CK_SheetTableRowRevisions_PublishedDateTime] CHECK ([Status]=(0) AND [PublishedDateTime] IS NULL OR [Status]=(1) AND [PublishedDateTime] IS NOT NULL),
    CONSTRAINT [CK_SheetTableRowRevisions_SupersededDateTime] CHECK ([SupersededDateTime] IS NULL OR [Status]=(1) AND [SupersededDateTime]>=[PublishedDateTime]),
    CONSTRAINT [FK_SheetTableRowRevisions_SheetTableRows_SheetTableRowID] FOREIGN KEY ([SheetTableRowID]) REFERENCES [spec].[SheetTableRows] ([SheetTableRowID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableRowRevisions_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]),
    CONSTRAINT [FK_SheetTableRowRevisions_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SheetTableRowRevisions_SheetVersions_SheetVersionID] FOREIGN KEY ([SheetVersionID]) REFERENCES [spec].[SheetVersions] ([SheetVersionID]),
    CONSTRAINT [FK_SheetTableRowRevisions_SheetSubmissions_SheetSubmissionID] FOREIGN KEY ([SheetSubmissionID]) REFERENCES [approval].[SheetSubmissions] ([SheetSubmissionID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableRowRevisions_SheetTableRowID_RevisionNumber]
    ON [spec].[SheetTableRowRevisions]([SheetTableRowID] ASC, [RevisionNumber] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableRowRevisions_OneDraft]
    ON [spec].[SheetTableRowRevisions]([SheetTableRowID] ASC) WHERE ([Status]=(0));

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableRowRevisions_OneCurrent]
    ON [spec].[SheetTableRowRevisions]([SheetTableRowID] ASC) WHERE ([Status]=(1) AND [SupersededDateTime] IS NULL);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRowRevisions_Published]
    ON [spec].[SheetTableRowRevisions]([SheetTableRowID] ASC, [PublishedDateTime] ASC)
    INCLUDE([SupersededDateTime]) WHERE ([Status]=(1));

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRowRevisions_SheetPublished]
    ON [spec].[SheetTableRowRevisions]([SheetID] ASC, [PublishedDateTime] ASC)
    INCLUDE([SupersededDateTime], [IsDeleted], [DisplayOrder], [SheetTableRowID]) WHERE ([Status]=(1));

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRowRevisions_SheetID_Status]
    ON [spec].[SheetTableRowRevisions]([SheetID] ASC, [Status] ASC)
    INCLUDE([SupersededDateTime], [SheetTableRowID], [CreatedByUserID]);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRowRevisions_CreatedByUserID_Status]
    ON [spec].[SheetTableRowRevisions]([CreatedByUserID] ASC, [Status] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRowRevisions_SheetVersionID]
    ON [spec].[SheetTableRowRevisions]([SheetVersionID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableRowRevisions_SheetSubmissionID]
    ON [spec].[SheetTableRowRevisions]([SheetSubmissionID] ASC);

