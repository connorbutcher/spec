CREATE TABLE [approval].[SheetSubmissions] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [SheetId]           INT             NOT NULL,
    [Note]              NVARCHAR (1000) NULL,
    [SubmittedByUserId] INT             NOT NULL,
    [SubmittedAtUtc]    DATETIME2 (7)   NOT NULL,
    [Status]            INT             NOT NULL,
    [DecidedAtUtc]      DATETIME2 (7)   NULL,
    [SheetVersionId]    INT             NULL,
    [RowVersion]        ROWVERSION      NOT NULL,
    CONSTRAINT [PK_SheetSubmissions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetSubmissions_DecidedAt] CHECK ([Status]=(0) AND [DecidedAtUtc] IS NULL OR [Status]<>(0) AND [DecidedAtUtc] IS NOT NULL),
    CONSTRAINT [CK_SheetSubmissions_Status] CHECK ([Status]=(3) OR [Status]=(2) OR [Status]=(1) OR [Status]=(0)),
    CONSTRAINT [CK_SheetSubmissions_Version] CHECK ([SheetVersionId] IS NULL OR [Status]=(1)),
    CONSTRAINT [FK_SheetSubmissions_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetSubmissions_SheetVersions_SheetVersionId] FOREIGN KEY ([SheetVersionId]) REFERENCES [spec].[SheetVersions] ([Id]),
    CONSTRAINT [FK_SheetSubmissions_Users_SubmittedByUserId] FOREIGN KEY ([SubmittedByUserId]) REFERENCES [auth].[Users] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSubmissions_SheetId_Status]
    ON [approval].[SheetSubmissions]([SheetId] ASC, [Status] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSubmissions_SheetVersionId]
    ON [approval].[SheetSubmissions]([SheetVersionId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetSubmissions_SubmittedByUserId_Status]
    ON [approval].[SheetSubmissions]([SubmittedByUserId] ASC, [Status] ASC);


GO

