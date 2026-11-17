CREATE TABLE [approval].[SheetSubmissionApprovers] (
    [SheetSubmissionID] INT            NOT NULL,
    [UserID]            SMALLINT       NOT NULL,
    [Decision]          TINYINT        NOT NULL,
    [DecidedDateTime]   DATETIME2 (7)  NULL,
    [Reason]            NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_SheetSubmissionApprovers] PRIMARY KEY CLUSTERED ([SheetSubmissionID] ASC, [UserID] ASC),
    CONSTRAINT [CK_SheetSubmissionApprovers_Decision] CHECK ([Decision]>=(0) AND [Decision]<=(2)),
    CONSTRAINT [CK_SheetSubmissionApprovers_DecidedDateTime] CHECK ([Decision]=(0) AND [DecidedDateTime] IS NULL OR [Decision]<>(0) AND [DecidedDateTime] IS NOT NULL),
    CONSTRAINT [CK_SheetSubmissionApprovers_Reason] CHECK ([Decision]<>(2) OR len([Reason])>(0)),
    CONSTRAINT [FK_SheetSubmissionApprovers_SheetSubmissions_SheetSubmissionID] FOREIGN KEY ([SheetSubmissionID]) REFERENCES [approval].[SheetSubmissions] ([SheetSubmissionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetSubmissionApprovers_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SheetSubmissionApprovers_UserID_Decision]
    ON [approval].[SheetSubmissionApprovers]([UserID] ASC, [Decision] ASC);

