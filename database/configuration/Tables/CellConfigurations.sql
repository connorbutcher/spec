CREATE TABLE [configuration].[CellConfigurations] (
    [CellConfigurationID] INT IDENTITY (1, 1) NOT NULL,
    [Styling]             NVARCHAR (MAX) NOT NULL,
    [Properties]          NVARCHAR (MAX) NOT NULL,
    [Validators]          NVARCHAR (MAX) NULL,
    [IsIdentifier]        BIT NOT NULL,
    CONSTRAINT [PK_CellConfigurations] PRIMARY KEY CLUSTERED ([CellConfigurationID] ASC)
);

