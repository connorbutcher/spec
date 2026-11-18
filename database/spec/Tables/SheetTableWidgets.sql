CREATE TABLE [spec].[SheetTableWidgets] (
    [SheetTableWidgetID]    INT           IDENTITY (1, 1) NOT NULL,
    [SheetTableID]          INT           NOT NULL,
    [TableTemplateWidgetID] SMALLINT      NOT NULL,
    [CreatedDateTime]       DATETIME2 (7) CONSTRAINT [DF_SheetTableWidgets_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetTableWidgets] PRIMARY KEY CLUSTERED ([SheetTableWidgetID] ASC),
    CONSTRAINT [FK_SheetTableWidgets_SheetTables_SheetTableID] FOREIGN KEY ([SheetTableID]) REFERENCES [spec].[SheetTables] ([SheetTableID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableWidgets_TableTemplateWidgets_TableTemplateWidgetID] FOREIGN KEY ([TableTemplateWidgetID]) REFERENCES [template].[TableTemplateWidgets] ([TableTemplateWidgetID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableWidgets_SheetTableID_TableTemplateWidgetID]
    ON [spec].[SheetTableWidgets]([SheetTableID] ASC, [TableTemplateWidgetID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableWidgets_TableTemplateWidgetID]
    ON [spec].[SheetTableWidgets]([TableTemplateWidgetID] ASC);
