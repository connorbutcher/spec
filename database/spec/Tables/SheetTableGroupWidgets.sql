CREATE TABLE [spec].[SheetTableGroupWidgets] (
    [SheetTableGroupWidgetID]    INT           IDENTITY (1, 1) NOT NULL,
    [SheetTableGroupID]          INT           NOT NULL,
    [TableGroupTemplateWidgetID] SMALLINT      NOT NULL,
    [CreatedDateTime]            DATETIME2 (7) CONSTRAINT [DF_SheetTableGroupWidgets_CreatedDateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_SheetTableGroupWidgets] PRIMARY KEY CLUSTERED ([SheetTableGroupWidgetID] ASC),
    CONSTRAINT [FK_SheetTableGroupWidgets_SheetTableGroups_SheetTableGroupID] FOREIGN KEY ([SheetTableGroupID]) REFERENCES [spec].[SheetTableGroups] ([SheetTableGroupID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SheetTableGroupWidgets_TableGroupTemplateWidgets_TableGroupTemplateWidgetID] FOREIGN KEY ([TableGroupTemplateWidgetID]) REFERENCES [template].[TableGroupTemplateWidgets] ([TableGroupTemplateWidgetID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_SheetTableGroupWidgets_SheetTableGroupID_TableGroupTemplateWidgetID]
    ON [spec].[SheetTableGroupWidgets]([SheetTableGroupID] ASC, [TableGroupTemplateWidgetID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SheetTableGroupWidgets_TableGroupTemplateWidgetID]
    ON [spec].[SheetTableGroupWidgets]([TableGroupTemplateWidgetID] ASC);
