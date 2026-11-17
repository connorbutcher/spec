CREATE TABLE [references].[SpecSheetExternalReferences] (
    [SpecSheetExternalReferenceID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetID]                  INT NOT NULL,
    [ExternalID]                   INT NOT NULL,
    [ReferenceID]                  UNIQUEIDENTIFIER NOT NULL,
    [SystemName]                   VARCHAR (256) NOT NULL,
    [OwnerName]                    VARCHAR (256) NOT NULL,
    [URL]                          NVARCHAR (MAX) NOT NULL,
    [Type]                         VARCHAR (32) NOT NULL,
    [Grouping]                     NVARCHAR (MAX) NULL,
    [SystemIconURL]                NVARCHAR (MAX) NULL,
    [Description]                  NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_SpecSheetExternalReferences] PRIMARY KEY CLUSTERED ([SpecSheetExternalReferenceID] ASC),
    CONSTRAINT [FK_SpecSheetExternalReferences_SpecSheets] FOREIGN KEY ([SpecSheetID]) REFERENCES [dbo].[SpecSheets] ([SpecSheetID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetExternalReferences_SpecSheetID]
    ON [references].[SpecSheetExternalReferences]([SpecSheetID] ASC);

