CREATE TABLE [values].[TableWidgetValues] (
    [TableWidgetValueID]     INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableWidgetID] INT NOT NULL,
    [Value]                  NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_TableWidgetValues] PRIMARY KEY CLUSTERED ([TableWidgetValueID] ASC),
    CONSTRAINT [FK_TableWidgetValues_SpecSheetTableWidgets] FOREIGN KEY ([SpecSheetTableWidgetID]) REFERENCES [dbo].[SpecSheetTableWidgets] ([SpecSheetTableWidgetID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TableWidgetValues_SpecSheetTableWidgetID]
    ON [values].[TableWidgetValues]([SpecSheetTableWidgetID] ASC);

