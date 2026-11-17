CREATE TABLE [dbo].[SpecSheetPropertyValues] (
    [SpecSheetPropertyValueID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetID]              INT NOT NULL,
    [SpecSheetTypePropertyID]  INT NOT NULL,
    [PropertyValue]            NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_SpecSheetPropertyValues] PRIMARY KEY CLUSTERED ([SpecSheetPropertyValueID] ASC),
    CONSTRAINT [FK_SpecSheetPropertyValues_SpecSheets] FOREIGN KEY ([SpecSheetID]) REFERENCES [dbo].[SpecSheets] ([SpecSheetID]),
    CONSTRAINT [FK_SpecSheetPropertyValues_SpecSheetTypeProperties] FOREIGN KEY ([SpecSheetTypePropertyID]) REFERENCES [types].[SpecSheetTypeProperties] ([SpecSheetTypePropertyID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetPropertyValues_SpecSheetID]
    ON [dbo].[SpecSheetPropertyValues]([SpecSheetID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetPropertyValues_SpecSheetTypePropertyID]
    ON [dbo].[SpecSheetPropertyValues]([SpecSheetTypePropertyID] ASC);

