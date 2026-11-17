CREATE TABLE [design].[WidgetTypes] (
    [Id]          INT              IDENTITY (1, 1) NOT NULL,
    [Key]         UNIQUEIDENTIFIER NOT NULL,
    [Name]        NVARCHAR (64)    NOT NULL,
    [Description] NVARCHAR (256)   NOT NULL,
    CONSTRAINT [PK_WidgetTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_WidgetTypes_Key]
    ON [design].[WidgetTypes]([Key] ASC);


GO
