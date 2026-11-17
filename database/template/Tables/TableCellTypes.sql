CREATE TABLE [template].[TableCellTypes] (
    [TableCellTypeID]           TINYINT IDENTITY (1, 1) NOT NULL,
    [TableCellTypeKey]          UNIQUEIDENTIFIER NOT NULL,
    [Title]                     VARCHAR (64) NOT NULL,
    [Description]               VARCHAR (256) NOT NULL,
    [Type]                      VARCHAR (64) NOT NULL,
    [ModelType]                 VARCHAR (32) NOT NULL,
    [DefaultStyling]            NVARCHAR (MAX) NOT NULL,
    [RequiresUserConfiguration] BIT CONSTRAINT [DF_TableCellTypes_RequiresUserConfiguration] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TableCellTypes] PRIMARY KEY CLUSTERED ([TableCellTypeID] ASC)
);

