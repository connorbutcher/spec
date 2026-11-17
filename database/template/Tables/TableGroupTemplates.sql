CREATE TABLE [template].[TableGroupTemplates] (
    [TableGroupTemplateID]       INT IDENTITY (1, 1) NOT NULL,
    [TableTemplateID]            TINYINT NOT NULL,
    [ParentTableGroupTemplateID] INT NULL,
    [Title]                      VARCHAR (128) NOT NULL,
    [GroupKey]                   VARCHAR (64) NULL,
    [GroupType]                  VARCHAR (32) NOT NULL,
    [DisplayOrder]               TINYINT NOT NULL,
    [IsDefault]                  BIT CONSTRAINT [DF_TableGroupTemplates_IsDefault] DEFAULT ((0)) NOT NULL,
    [CanUserAddToTable]          BIT CONSTRAINT [DF_TableGroupTemplates_CanUserAddToTable] DEFAULT ((1)) NOT NULL,
    [ShowInContextMenu]          BIT CONSTRAINT [DF_TableGroupTemplates_ShowInContextMenu] DEFAULT ((1)) NOT NULL,
    [CanBeReordered]             BIT CONSTRAINT [DF_TableGroupTemplates_CanBeReordered] DEFAULT ((1)) NOT NULL,
    [CopyFromPrevious]           BIT CONSTRAINT [DF_TableGroupTemplates_CopyFromPrevious] DEFAULT ((0)) NOT NULL,
    [ShowContextMenu]            BIT CONSTRAINT [DF_TableGroupTemplates_ShowContextMenu] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_TableGroupTemplates] PRIMARY KEY CLUSTERED ([TableGroupTemplateID] ASC),
    CONSTRAINT [FK_TableGroupTemplates_TableTemplates] FOREIGN KEY ([TableTemplateID]) REFERENCES [template].[TableTemplates] ([TableTemplateID]),
    CONSTRAINT [FK_TableGroupTemplates_TableGroupTemplates_ParentTableGroupTemplateID] FOREIGN KEY ([ParentTableGroupTemplateID]) REFERENCES [template].[TableGroupTemplates] ([TableGroupTemplateID])
);

GO
CREATE NONCLUSTERED INDEX [IX_TableGroupTemplates_TableTemplateID]
    ON [template].[TableGroupTemplates]([TableTemplateID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_TableGroupTemplates_ParentTableGroupTemplateID]
    ON [template].[TableGroupTemplates]([ParentTableGroupTemplateID] ASC);

