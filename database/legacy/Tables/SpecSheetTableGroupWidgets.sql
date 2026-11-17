CREATE TABLE [legacy].[SpecSheetTableGroupWidgets] (
    [SpecSheetTableGroupWidgetID] INT IDENTITY (1, 1) NOT NULL,
    [SpecSheetTableGroupID]       INT NOT NULL,
    [TableGroupTemplateWidgetID]  SMALLINT NOT NULL,
    CONSTRAINT [PK_SpecSheetTableGroupWidgets] PRIMARY KEY CLUSTERED ([SpecSheetTableGroupWidgetID] ASC),
    CONSTRAINT [FK_SpecSheetTableGroupWidgets_SpecSheetTableGroups] FOREIGN KEY ([SpecSheetTableGroupID]) REFERENCES [legacy].[SpecSheetTableGroups] ([SpecSheetTableGroupID]),
    CONSTRAINT [FK_SpecSheetTableGroupWidgets_TableTemplateGroupWidgets] FOREIGN KEY ([TableGroupTemplateWidgetID]) REFERENCES [template].[TableGroupTemplateWidgets] ([TableGroupTemplateWidgetID])
);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableGroupWidgets_SpecSheetTableGroupID]
    ON [legacy].[SpecSheetTableGroupWidgets]([SpecSheetTableGroupID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_SpecSheetTableGroupWidgets_TableGroupTemplateWidgetID]
    ON [legacy].[SpecSheetTableGroupWidgets]([TableGroupTemplateWidgetID] ASC);

