CREATE TABLE [types].[SpecSheetTypeStructureLevelValues] (
    [SpecSheetTypeStructureLevelValueID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTypeStructureLevelID]      INT NOT NULL,
    [SpecSheetTypePropertyReferenceID]   UNIQUEIDENTIFIER NOT NULL,
    [DisplayOrder]                       TINYINT NOT NULL,
    CONSTRAINT [PK_SpecSheetTypeStructureLevelValues] PRIMARY KEY CLUSTERED ([SpecSheetTypeStructureLevelValueID] ASC),
    CONSTRAINT [FK_SpecSheetTypeStructureLevelValues_SpecSheetTypeStructureLevels] FOREIGN KEY ([SpecSheetTypeStructureLevelID]) REFERENCES [types].[SpecSheetTypeStructureLevels] ([SpecSheetTypeStructureLevelID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTypeStructureLevelValues_SpecSheetTypeStructureLevelID]
    ON [types].[SpecSheetTypeStructureLevelValues]([SpecSheetTypeStructureLevelID] ASC);

