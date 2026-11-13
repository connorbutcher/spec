CREATE TABLE [template].[CellTypeOptions] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [CellTypeId]   INT            NOT NULL,
    [Value]        NVARCHAR (100) NOT NULL,
    [DisplayOrder] INT            NOT NULL,
    CONSTRAINT [PK_CellTypeOptions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CellTypeOptions_CellTypes_CellTypeId] FOREIGN KEY ([CellTypeId]) REFERENCES [template].[CellTypes] ([Id]) ON DELETE CASCADE
);


GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_CellTypeOptions_CellTypeId_Value]
    ON [template].[CellTypeOptions]([CellTypeId] ASC, [Value] ASC);


GO

