CREATE TABLE [spec].[SheetVersions] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [SheetId]           INT             NOT NULL,
    [VersionNumber]     INT             NOT NULL,
    [PublishedAtUtc]    DATETIME2 (7)   NOT NULL,
    [PublishedByUserId] INT             NOT NULL,
    [Note]              NVARCHAR (1000) NULL,
    CONSTRAINT [PK_SheetVersions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_SheetVersions_VersionNumber] CHECK ([VersionNumber]>=(1)),
    CONSTRAINT [FK_SheetVersions_Sheets_SheetId] FOREIGN KEY ([SheetId]) REFERENCES [spec].[Sheets] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetVersions_Users_PublishedByUserId] FOREIGN KEY ([PublishedByUserId]) REFERENCES [auth].[Users] ([Id])
);


GO

CREATE NONCLUSTERED INDEX [IX_SheetVersions_SheetId_PublishedAtUtc]
    ON [spec].[SheetVersions]([SheetId] ASC, [PublishedAtUtc] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_SheetVersions_PublishedByUserId]
    ON [spec].[SheetVersions]([PublishedByUserId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SheetVersions_SheetId_VersionNumber]
    ON [spec].[SheetVersions]([SheetId] ASC, [VersionNumber] ASC);


GO

