CREATE TABLE [template].[TableRowTemplates] (
    [TableRowTemplateID]   INT IDENTITY (1, 1) NOT NULL,
    [TableGroupTemplateID] INT NOT NULL,
    [RowNumber]            TINYINT NOT NULL,
    [RowValidators]        NVARCHAR (MAX) NULL,
    [ShowContextMenu]      BIT CONSTRAINT [DF_TableRowTemplates_ShowContextMenu] DEFAULT ((1)) NOT NULL,
    [CanBeReordered]       BIT CONSTRAINT [DF_TableRowTemplates_CanBeReordered] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_TableRowTemplates] PRIMARY KEY CLUSTERED ([TableRowTemplateID] ASC),
    CONSTRAINT [FK_TableRowTemplates_TableGroupTemplates] FOREIGN KEY ([TableGroupTemplateID]) REFERENCES [template].[TableGroupTemplates] ([TableGroupTemplateID])
);

GO
CREATE NONCLUSTERED INDEX [IX_TableRowTemplates_TableGroupTemplateID]
    ON [template].[TableRowTemplates]([TableGroupTemplateID] ASC);

