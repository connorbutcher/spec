CREATE TABLE [legacy].[SpecSheetTables] (
    [SpecSheetTableID]      INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetID]           INT NOT NULL,
    [TableTemplateID]       TINYINT NOT NULL,
    [Title]                 VARCHAR (64) NOT NULL,
    [DisplayOrder]          SMALLINT NOT NULL,
    [ReferenceID]           UNIQUEIDENTIFIER NOT NULL,
    [CreatedByUserID]       SMALLINT NOT NULL,
    [CreatedDateTime]       DATETIME2 (0) NOT NULL,
    [IsDeleted]             BIT CONSTRAINT [DF_SpecSheetTables_IsDeleted] DEFAULT ((0)) NOT NULL,
    [LastModifiedByUserID]  SMALLINT NULL,
    [LastModifiedDateTime]  DATETIME2 (0) NULL,
    [ConfigurationOverride] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_SpecSheetTables] PRIMARY KEY CLUSTERED ([SpecSheetTableID] ASC),
    CONSTRAINT [FK_SpecSheetTables_SpecSheets] FOREIGN KEY ([SpecSheetID]) REFERENCES [legacy].[SpecSheets] ([SpecSheetID]),
    CONSTRAINT [FK_SpecSheetTables_TableTemplates] FOREIGN KEY ([TableTemplateID]) REFERENCES [template].[TableTemplates] ([TableTemplateID]),
    CONSTRAINT [FK_SpecSheetTables_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [access].[Users] ([UserID]),
    CONSTRAINT [FK_SpecSheetTables_Users_LastModifiedByUserID] FOREIGN KEY ([LastModifiedByUserID]) REFERENCES [access].[Users] ([UserID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTables_SpecSheetID]
    ON [legacy].[SpecSheetTables]([SpecSheetID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTables_TableTemplateID]
    ON [legacy].[SpecSheetTables]([TableTemplateID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTables_CreatedByUserID]
    ON [legacy].[SpecSheetTables]([CreatedByUserID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTables_LastModifiedByUserID]
    ON [legacy].[SpecSheetTables]([LastModifiedByUserID] ASC);

