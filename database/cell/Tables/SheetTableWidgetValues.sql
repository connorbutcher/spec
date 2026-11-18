CREATE TABLE [cell].[SheetTableWidgetValues] (
    [SheetTableRevisionID] INT            NOT NULL,
    [SheetTableWidgetID]   INT            NOT NULL,
    [Value]                NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_SheetTableWidgetValues] PRIMARY KEY CLUSTERED ([SheetTableRevisionID] ASC, [SheetTableWidgetID] ASC),
    CONSTRAINT [CK_SheetTableWidgetValues_Value] CHECK ([Value] IS NULL OR isjson([Value])=(1)),
    CONSTRAINT [FK_SheetTableWidgetValues_SheetTableRevisions_SheetTableRevisionID] FOREIGN KEY ([SheetTableRevisionID]) REFERENCES [spec].[SheetTableRevisions] ([SheetTableRevisionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableWidgetValues_SheetTableWidgets_SheetTableWidgetID] FOREIGN KEY ([SheetTableWidgetID]) REFERENCES [spec].[SheetTableWidgets] ([SheetTableWidgetID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableWidgetValues_SheetTableWidgetID]
    ON [cell].[SheetTableWidgetValues]([SheetTableWidgetID] ASC);
