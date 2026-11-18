CREATE TABLE [cell].[TableWidgetValues] (
    [SheetTableRevisionID] INT            NOT NULL,
    [SheetTableWidgetID]   INT            NOT NULL,
    [Value]                NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_TableWidgetValues] PRIMARY KEY CLUSTERED ([SheetTableRevisionID] ASC, [SheetTableWidgetID] ASC),
    CONSTRAINT [CK_TableWidgetValues_Value] CHECK ([Value] IS NULL OR isjson([Value])=(1)),
    CONSTRAINT [FK_TableWidgetValues_SheetTableRevisions_SheetTableRevisionID] FOREIGN KEY ([SheetTableRevisionID]) REFERENCES [spec].[SheetTableRevisions] ([SheetTableRevisionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_TableWidgetValues_SheetTableWidgets_SheetTableWidgetID] FOREIGN KEY ([SheetTableWidgetID]) REFERENCES [spec].[SheetTableWidgets] ([SheetTableWidgetID])
);

GO
CREATE NONCLUSTERED INDEX [IX_TableWidgetValues_SheetTableWidgetID]
    ON [cell].[TableWidgetValues]([SheetTableWidgetID] ASC);
