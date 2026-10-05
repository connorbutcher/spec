CREATE TABLE [dbo].[TemplateCells] (
    [Id]                    INT            IDENTITY (1, 1) NOT NULL,
    [TemplateRowId]         INT            NOT NULL,
    [Column]                INT            NOT NULL,
    [RowSpan]               INT            DEFAULT ((1)) NOT NULL,
    [ColumnSpan]            INT            DEFAULT ((1)) NOT NULL,
    [Caption]               NVARCHAR (200) NULL,
    [CellTypeId]            INT            DEFAULT ((2)) NOT NULL,
    [IsRequired]            BIT            DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [ConfigurationOverride] NVARCHAR (MAX) NULL,
    [StyleOverride]         NVARCHAR (MAX) NULL,
    [TemplateColumnBlockId] INT            NULL,
    [LookupKey]             NVARCHAR (50)  NULL,
    CONSTRAINT [PK_TemplateCells] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_TemplateCells_Column] CHECK ([Column]>=(1)),
    CONSTRAINT [CK_TemplateCells_ColumnSpan] CHECK ([ColumnSpan]>=(1)),
    CONSTRAINT [CK_TemplateCells_RowSpan] CHECK ([RowSpan]>=(1)),
    CONSTRAINT [FK_TemplateCells_CellTypes_CellTypeId] FOREIGN KEY ([CellTypeId]) REFERENCES [dbo].[CellTypes] ([Id]),
    CONSTRAINT [FK_TemplateCells_TemplateColumnBlocks_TemplateColumnBlockId] FOREIGN KEY ([TemplateColumnBlockId]) REFERENCES [dbo].[TemplateColumnBlocks] ([Id]),
    CONSTRAINT [FK_TemplateCells_TemplateRows_TemplateRowId] FOREIGN KEY ([TemplateRowId]) REFERENCES [dbo].[TemplateRows] ([Id]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_TemplateCells_TemplateColumnBlockId]
    ON [dbo].[TemplateCells]([TemplateColumnBlockId] ASC);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_TemplateCells_TemplateRowId_TemplateColumnBlockId_Column]
    ON [dbo].[TemplateCells]([TemplateRowId] ASC, [TemplateColumnBlockId] ASC, [Column] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_TemplateCells_LookupKey]
    ON [dbo].[TemplateCells]([LookupKey] ASC) WHERE ([LookupKey] IS NOT NULL);


GO

CREATE NONCLUSTERED INDEX [IX_TemplateCells_CellTypeId]
    ON [dbo].[TemplateCells]([CellTypeId] ASC);


GO

