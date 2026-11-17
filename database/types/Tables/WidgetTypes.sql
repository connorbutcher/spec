CREATE TABLE [types].[WidgetTypes] (
    [WidgetTypeID]  TINYINT IDENTITY (1, 1) NOT NULL,
    [WidgetTypeKey] UNIQUEIDENTIFIER NOT NULL,
    [Title]         VARCHAR (64) NOT NULL,
    [Description]   VARCHAR (256) NOT NULL,
    CONSTRAINT [PK_WidgetTypes] PRIMARY KEY CLUSTERED ([WidgetTypeID] ASC)
);

