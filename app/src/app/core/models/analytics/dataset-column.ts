import { DatasetColumnType } from './dataset-column-type.enum';

export type DatasetColumn = {
  id: number;
  columnKey: string;
  displayName: string;
  dataType: DatasetColumnType;
  displayOrder: number;
  isFormula: boolean;
  isMetadata: boolean;
  configuration: string | null;
  formula: string | null;
};
