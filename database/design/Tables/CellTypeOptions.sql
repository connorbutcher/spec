CREATE TABLE [design].[CellTypeOptions] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [CellTypeId]   INT            NOT NULL,
    [Value]        NVARCHAR (100) NOT NULL,
    [DisplayOrder] INT            NOT NULL,
    CONSTRAINT [PK_CellTypeOptions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CellTypeOptions_CellTypes_CellTypeId] FOREIGN KEY ([CellTypeId]) REFERENCES [design].[CellTypes] ([Id]) ON DELETE CASCADE
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_CellTypeOptions_CellTypeId_Value]
    ON [design].[CellTypeOptions]([CellTypeId] ASC, [Value] ASC);


GO

