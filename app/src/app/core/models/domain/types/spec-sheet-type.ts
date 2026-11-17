import { SpecSheetTypePermission } from '../access/spec-sheet-type-permission';
import { SpecSheetTypeStructureLevel } from '../configuration/spec-sheet-type-structure-level';
import { TableTemplate } from '../template/table-template';
import { SpecSheetTypeProperty } from './spec-sheet-type-property';

export type TableOrientation = 'horizontal' | 'vertical';

export type SpecSheetType = {
  id: number;
  type: string;
  tableOrientation: TableOrientation;
  displayOrder: number;
  canCopyFromMigrated: boolean;
  requiresApprovalWorkflow: boolean;
  tableTemplates: TableTemplate[];
  permissions: SpecSheetTypePermission[];
  properties: SpecSheetTypeProperty[];
  structureLevels: SpecSheetTypeStructureLevel[];
};
