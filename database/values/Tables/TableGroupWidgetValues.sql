CREATE TABLE [values].[TableGroupWidgetValues] (
    [TableGroupWidgetValueID]     INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableGroupWidgetID] INT NOT NULL,
    [Value]                       NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_TableGroupWidgetValues] PRIMARY KEY CLUSTERED ([TableGroupWidgetValueID] ASC),
    CONSTRAINT [FK_TableGroupWidgetValues_SpecSheetTableGroupWidgets] FOREIGN KEY ([SpecSheetTableGroupWidgetID]) REFERENCES [dbo].[SpecSheetTableGroupWidgets] ([SpecSheetTableGroupWidgetID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TableGroupWidgetValues_SpecSheetTableGroupWidgetID]
    ON [values].[TableGroupWidgetValues]([SpecSheetTableGroupWidgetID] ASC);

