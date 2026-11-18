CREATE TABLE [cell].[TableGroupWidgetValues] (
    [SheetTableGroupRevisionID] INT            NOT NULL,
    [SheetTableGroupWidgetID]   INT            NOT NULL,
    [Value]                     NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_TableGroupWidgetValues] PRIMARY KEY CLUSTERED ([SheetTableGroupRevisionID] ASC, [SheetTableGroupWidgetID] ASC),
    CONSTRAINT [CK_TableGroupWidgetValues_Value] CHECK ([Value] IS NULL OR isjson([Value])=(1)),
    CONSTRAINT [FK_TableGroupWidgetValues_SheetTableGroupRevisions_SheetTableGroupRevisionID] FOREIGN KEY ([SheetTableGroupRevisionID]) REFERENCES [spec].[SheetTableGroupRevisions] ([SheetTableGroupRevisionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_TableGroupWidgetValues_SheetTableGroupWidgets_SheetTableGroupWidgetID] FOREIGN KEY ([SheetTableGroupWidgetID]) REFERENCES [spec].[SheetTableGroupWidgets] ([SheetTableGroupWidgetID])
);

GO
CREATE NONCLUSTERED INDEX [IX_TableGroupWidgetValues_SheetTableGroupWidgetID]
    ON [cell].[TableGroupWidgetValues]([SheetTableGroupWidgetID] ASC);
