CREATE TABLE [dbo].[SheetSubmissionApprovers] (
    [SheetSubmissionId] INT             NOT NULL,
    [UserId]            INT             NOT NULL,
    [Decision]          INT             NOT NULL,
    [DecidedAtUtc]      DATETIME2 (7)   NULL,
    [Reason]            NVARCHAR (1000) NULL,
    CONSTRAINT [PK_SheetSubmissionApprovers] PRIMARY KEY CLUSTERED ([SheetSubmissionId] ASC, [UserId] ASC),
    CONSTRAINT [CK_SheetSubmissionApprovers_Decision] CHECK ([Decision]=(2) OR [Decision]=(1) OR [Decision]=(0)),
    CONSTRAINT [CK_SheetSubmissionApprovers_Reason] CHECK ([Decision]<>(2) OR len([Reason])>(0)),
    CONSTRAINT [FK_SheetSubmissionApprovers_SheetSubmissions_SheetSubmissionId] FOREIGN KEY ([SheetSubmissionId]) REFERENCES [dbo].[SheetSubmissions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetSubmissionApprovers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSubmissionApprovers_UserId_Decision]
    ON [dbo].[SheetSubmissionApprovers]([UserId] ASC, [Decision] ASC);


GO

