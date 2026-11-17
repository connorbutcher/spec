CREATE TABLE [spec].[SheetExternalReferenceRevisions] (
    [SheetExternalReferenceRevisionID] INT            IDENTITY (1, 1) NOT NULL,
    [SheetExternalReferenceID]         INT            NOT NULL,
    [SheetID]                          INT            NOT NULL,
    [SheetVersionID]                   INT            NOT NULL,
    [RevisionNumber]                   INT            NOT NULL,
    [ExternalID]                       INT            NOT NULL,
    [SystemName]                       VARCHAR (256)  NOT NULL,
    [OwnerName]                        VARCHAR (256)  NOT NULL,
    [URL]                              NVARCHAR (MAX) NOT NULL,
    [Type]                             VARCHAR (32)   NOT NULL,
    [Grouping]                         NVARCHAR (MAX) NULL,
    [SystemIconURL]                    NVARCHAR (MAX) NULL,
    [Description]                      NVARCHAR (MAX) NULL,
    [IsDeleted]                        BIT            CONSTRAINT [DF_SheetExternalReferenceRevisions_IsDeleted] DEFAULT ((0)) NOT NULL,
    [CreatedDateTime]                  DATETIME2 (7)  CONSTRAINT [DF_SheetExternalReferenceRevisions_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [PublishedDateTime]                DATETIME2 (7)  NOT NULL,
    [SupersededDateTime]               DATETIME2 (7)  NULL,
    [RowVersion]                       ROWVERSION     NOT NULL,
    CONSTRAINT [PK_SheetExternalReferenceRevisions] PRIMARY KEY CLUSTERED ([SheetExternalReferenceRevisionID] ASC),
    CONSTRAINT [CK_SheetExternalReferenceRevisions_RevisionNumber] CHECK ([RevisionNumber]>=(1)),
    CONSTRAINT [CK_SheetExternalReferenceRevisions_SupersededDateTime] CHECK ([SupersededDateTime] IS NULL OR [SupersededDateTime]>=[PublishedDateTime]),
    CONSTRAINT [FK_SheetExternalReferenceRevisions_SheetExternalReferences_SheetExternalReferenceID] FOREIGN KEY ([SheetExternalReferenceID]) REFERENCES [spec].[SheetExternalReferences] ([SheetExternalReferenceID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetExternalReferenceRevisions_Sheets_SheetID] FOREIGN KEY ([SheetID]) REFERENCES [spec].[Sheets] ([SheetID]),
    CONSTRAINT [FK_SheetExternalReferenceRevisions_SheetVersions_SheetVersionID] FOREIGN KEY ([SheetVersionID]) REFERENCES [spec].[SheetVersions] ([SheetVersionID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetExternalReferenceRevisions_SheetExternalReferenceID_RevisionNumber]
    ON [spec].[SheetExternalReferenceRevisions]([SheetExternalReferenceID] ASC, [RevisionNumber] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetExternalReferenceRevisions_OnePerVersion]
    ON [spec].[SheetExternalReferenceRevisions]([SheetExternalReferenceID] ASC, [SheetVersionID] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetExternalReferenceRevisions_OneCurrent]
    ON [spec].[SheetExternalReferenceRevisions]([SheetExternalReferenceID] ASC) WHERE ([SupersededDateTime] IS NULL);

GO
CREATE NONCLUSTERED INDEX [IX_SheetExternalReferenceRevisions_SheetPublished]
    ON [spec].[SheetExternalReferenceRevisions]([SheetID] ASC, [PublishedDateTime] ASC)
    INCLUDE([SupersededDateTime], [IsDeleted], [SheetExternalReferenceID]);

GO
CREATE NONCLUSTERED INDEX [IX_SheetExternalReferenceRevisions_SheetVersionID]
    ON [spec].[SheetExternalReferenceRevisions]([SheetVersionID] ASC);

