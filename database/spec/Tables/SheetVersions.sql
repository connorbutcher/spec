CREATE TABLE [spec].[SheetVersions] (
    [SheetVersionID]        INT            IDENTITY (1, 1) NOT NULL,
    [SheetID]               INT            NOT NULL,
    [VersionNumber]         INT            NOT NULL,
    [PublishedDateTime]     DATETIME2 (7)  NOT NULL,
    [PublishedByUserID]     SMALLINT       NOT NULL,
    [ChangeLog]             NVARCHAR (MAX) NULL,
    [SourceSpecSheetID]     INT            NULL,
    [LegacySpecSheetID]     INT            NULL,
    [HasBeenMigrated]       BIT            CONSTRAINT [DF_SheetVersions_HasBeenMigrated] DEFAULT ((0)) NOT NULL,
    [SourceCreatedByUserID] SMALLINT       NULL,
    [SourceCreatedDateTime] DATETIME2 (7)  NULL,
    CONSTRAINT [PK_SheetVersions] PRIMARY KEY CLUSTERED ([SheetVersionID] ASC),
    CONSTRAINT [CK_SheetVersions_VersionNumber] CHECK ([VersionNumber]>=(1)),
    CONSTRAINT [FK_SheetVersions_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetVersions_Users_PublishedByUserID] FOREIGN KEY ([PublishedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SheetVersions_Users_SourceCreatedByUserID] FOREIGN KEY ([SourceCreatedByUserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetVersions_SheetID_VersionNumber]
    ON [spec].[SheetVersions]([SheetID] ASC, [VersionNumber] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetVersions_SheetID_PublishedDateTime]
    ON [spec].[SheetVersions]([SheetID] ASC, [PublishedDateTime] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetVersions_PublishedByUserID]
    ON [spec].[SheetVersions]([PublishedByUserID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetVersions_SourceSpecSheetID]
    ON [spec].[SheetVersions]([SourceSpecSheetID] ASC);

