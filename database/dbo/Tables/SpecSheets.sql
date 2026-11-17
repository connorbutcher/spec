CREATE TABLE [dbo].[SpecSheets] (
    [SpecSheetID]       INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetPhaseID]  SMALLINT NOT NULL,
    [SpecSheetTypeID]   TINYINT NOT NULL,
    [IsPublished]       BIT CONSTRAINT [DF_SpecSheets_IsPublished] DEFAULT ((0)) NOT NULL,
    [CreatedByUserID]   SMALLINT NOT NULL,
    [CreatedDateTime]   DATETIME2 (0) NOT NULL,
    [PublishedByUserID] SMALLINT NULL,
    [PublishedDateTime] DATETIME2 (0) NULL,
    [ChangeLog]         NVARCHAR (MAX) NULL,
    [HasBeenMigrated]   BIT CONSTRAINT [DF_SpecSheets_HasBeenMigrated] DEFAULT ((0)) NOT NULL,
    [LegacySpecSheetID] INT NULL,
    CONSTRAINT [PK_SpecSheets] PRIMARY KEY CLUSTERED ([SpecSheetID] ASC),
    CONSTRAINT [FK_SpecSheets_SpecSheetPhases] FOREIGN KEY ([SpecSheetPhaseID]) REFERENCES [types].[SpecSheetPhases] ([SpecSheetPhaseID]),
    CONSTRAINT [FK_SpecSheets_SpecSheetTypes] FOREIGN KEY ([SpecSheetTypeID]) REFERENCES [types].[SpecSheetTypes] ([SpecSheetTypeID]),
    CONSTRAINT [FK_SpecSheets_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SpecSheets_Users_PublishedByUserID] FOREIGN KEY ([PublishedByUserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheets_SpecSheetPhaseID]
    ON [dbo].[SpecSheets]([SpecSheetPhaseID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheets_SpecSheetTypeID]
    ON [dbo].[SpecSheets]([SpecSheetTypeID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheets_CreatedByUserID]
    ON [dbo].[SpecSheets]([CreatedByUserID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheets_PublishedByUserID]
    ON [dbo].[SpecSheets]([PublishedByUserID] ASC);

