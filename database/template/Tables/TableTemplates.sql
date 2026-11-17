CREATE TABLE [template].[TableTemplates] (
    [TableTemplateID]           TINYINT IDENTITY (1, 1) NOT NULL,
    [Type]                      VARCHAR (64) NOT NULL,
    [Description]               VARCHAR (2048) NOT NULL,
    [SpecSheetTypeID]           TINYINT NOT NULL,
    [RequiresUserConfiguration] BIT CONSTRAINT [DF_TableTemplates_RequiresUserConfiguration] DEFAULT ((0)) NULL,
    [DefaultConfiguration]      NVARCHAR (MAX) NULL,
    [ShowNotes]                 BIT CONSTRAINT [DF_TableTemplates_ShowNotes] DEFAULT ((0)) NOT NULL,
    [IsLegacy]                  BIT CONSTRAINT [DF_TableTemplates_IsLegacy] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TableTemplates] PRIMARY KEY CLUSTERED ([TableTemplateID] ASC),
    CONSTRAINT [FK_TableTemplates_SpecSheetTypes] FOREIGN KEY ([SpecSheetTypeID]) REFERENCES [types].[SpecSheetTypes] ([SpecSheetTypeID])
);

GO
CREATE NONCLUSTERED INDEX [IX_TableTemplates_SpecSheetTypeID]
    ON [template].[TableTemplates]([SpecSheetTypeID] ASC);

