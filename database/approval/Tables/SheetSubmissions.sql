CREATE TABLE [approval].[SheetSubmissions] (
    [SheetSubmissionID] INT            IDENTITY (1, 1) NOT NULL,
    [SheetID]           INT            NOT NULL,
    [Note]              NVARCHAR (MAX) NULL,
    [SubmittedByUserID] SMALLINT       NOT NULL,
    [SubmittedDateTime] DATETIME2 (7)  NOT NULL,
    [Status]            TINYINT        NOT NULL,
    [DecidedDateTime]   DATETIME2 (7)  NULL,
    [SheetVersionID]    INT            NULL,
    [RowVersion]        ROWVERSION     NOT NULL,
    CONSTRAINT [PK_SheetSubmissions] PRIMARY KEY CLUSTERED ([SheetSubmissionID] ASC),
    CONSTRAINT [CK_SheetSubmissions_Status] CHECK ([Status]>=(0) AND [Status]<=(3)),
    CONSTRAINT [CK_SheetSubmissions_DecidedDateTime] CHECK ([Status]=(0) AND [DecidedDateTime] IS NULL OR [Status]<>(0) AND [DecidedDateTime] IS NOT NULL),
    CONSTRAINT [CK_SheetSubmissions_SheetVersionID] CHECK ([SheetVersionID] IS NULL OR [Status]=(1)),
    CONSTRAINT [FK_SheetSubmissions_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetSubmissions_Users_SubmittedByUserID] FOREIGN KEY ([SubmittedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SheetSubmissions_SheetVersions_SheetVersionID] FOREIGN KEY ([SheetVersionID]) REFERENCES [spec].[SheetVersions] ([SheetVersionID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SheetSubmissions_SheetID_Status]
    ON [approval].[SheetSubmissions]([SheetID] ASC, [Status] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetSubmissions_SubmittedByUserID_Status]
    ON [approval].[SheetSubmissions]([SubmittedByUserID] ASC, [Status] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetSubmissions_SheetVersionID]
    ON [approval].[SheetSubmissions]([SheetVersionID] ASC);

