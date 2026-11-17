CREATE TABLE [types].[SpecSheetTypes] (
    [SpecSheetTypeID]          TINYINT IDENTITY (1, 1) NOT NULL,
    [Type]                     VARCHAR (64) NOT NULL,
    [DisplayOrder]             TINYINT NOT NULL,
    [TableOrientation]         VARCHAR (16) NOT NULL,
    [CanCopyFromMigrated]      BIT CONSTRAINT [DF_SpecSheetTypes_CanCopyFromMigrated] DEFAULT ((1)) NOT NULL,
    [RequiresApprovalWorkflow] BIT CONSTRAINT [DF_SpecSheetTypes_RequiresApprovalWorkflow] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_SpecSheetTypes] PRIMARY KEY CLUSTERED ([SpecSheetTypeID] ASC)
);

