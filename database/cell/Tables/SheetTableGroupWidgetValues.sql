CREATE TABLE [cell].[SheetTableGroupWidgetValues] (
    [SheetTableGroupRevisionID] INT            NOT NULL,
    [SheetTableGroupWidgetID]   INT            NOT NULL,
    [Value]                     NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_SheetTableGroupWidgetValues] PRIMARY KEY CLUSTERED ([SheetTableGroupRevisionID] ASC, [SheetTableGroupWidgetID] ASC),
    CONSTRAINT [CK_SheetTableGroupWidgetValues_Value] CHECK ([Value] IS NULL OR isjson([Value])=(1)),
    CONSTRAINT [FK_SheetTableGroupWidgetValues_SheetTableGroupRevisions_SheetTableGroupRevisionID] FOREIGN KEY ([SheetTableGroupRevisionID]) REFERENCES [spec].[SheetTableGroupRevisions] ([SheetTableGroupRevisionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableGroupWidgetValues_SheetTableGroupWidgets_SheetTableGroupWidgetID] FOREIGN KEY ([SheetTableGroupWidgetID]) REFERENCES [spec].[SheetTableGroupWidgets] ([SheetTableGroupWidgetID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableGroupWidgetValues_SheetTableGroupWidgetID]
    ON [cell].[SheetTableGroupWidgetValues]([SheetTableGroupWidgetID] ASC);
