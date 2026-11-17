import { TableProperties } from '../../domain/properties/table/table-properties';

export type AddSpecSheetTableOp = {
  $type: 'AddSpecSheetTableOp';
  tempId: number;
  specSheetId: number;
  tableTemplateId: number;
  title: string;
  displayOrder: number;
  referenceId: string;
  configurationOverride?: TableProperties;
};
