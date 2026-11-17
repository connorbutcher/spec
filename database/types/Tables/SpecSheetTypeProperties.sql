CREATE TABLE [types].[SpecSheetTypeProperties] (
    [SpecSheetTypePropertyID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTypeID]         TINYINT NOT NULL,
    [ValueName]               VARCHAR (256) NOT NULL,
    [ValueKey]                VARCHAR (256) NOT NULL,
    [ValueType]               VARCHAR (64) NOT NULL,
    [ValueControlType]        VARCHAR (64) NOT NULL,
    [CanValueBeSetInForm]     BIT CONSTRAINT [DF_SpecSheetTypeProperties_CanValueBeSetInForm] DEFAULT ((1)) NOT NULL,
    [DisplayOrder]            SMALLINT NOT NULL,
    [ReferenceID]             UNIQUEIDENTIFIER NOT NULL,
    [PlaceholderText]         VARCHAR (512) NULL,
    [ValidatorConfiguration]  NVARCHAR (MAX) NULL,
    [SaveValueAsProperty]     BIT CONSTRAINT [DF_SpecSheetTypeProperties_SaveValueAsProperty] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_SpecSheetTypeProperties] PRIMARY KEY CLUSTERED ([SpecSheetTypePropertyID] ASC),
    CONSTRAINT [FK_SpecSheetTypeProperties_SpecSheetTypes] FOREIGN KEY ([SpecSheetTypeID]) REFERENCES [types].[SpecSheetTypes] ([SpecSheetTypeID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTypeProperties_SpecSheetTypeID]
    ON [types].[SpecSheetTypeProperties]([SpecSheetTypeID] ASC);

