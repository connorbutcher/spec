CREATE TABLE [types].[SpecSheetTypeStructureLevels] (
    [SpecSheetTypeStructureLevelID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTypeID]               TINYINT NOT NULL,
    [Level]                         TINYINT NOT NULL,
    [SortOrientation]               TINYINT NOT NULL,
    [StructureLevelType]            TINYINT NOT NULL,
    CONSTRAINT [PK_SpecSheetTypeStructureLevels] PRIMARY KEY CLUSTERED ([SpecSheetTypeStructureLevelID] ASC),
    CONSTRAINT [FK_SpecSheetTypeStructureLevels_SpecSheetTypes] FOREIGN KEY ([SpecSheetTypeID]) REFERENCES [types].[SpecSheetTypes] ([SpecSheetTypeID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTypeStructureLevels_SpecSheetTypeID]
    ON [types].[SpecSheetTypeStructureLevels]([SpecSheetTypeID] ASC);

