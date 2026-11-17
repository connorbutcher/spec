import { DatasetColumn } from './dataset-column';
import { DatasetSource } from './dataset-source';
import { PUSpecSheetSourceConfiguration } from './pu-spec-sheet-source-configuration';

export type Dataset = {
  id: number;
  title: string;
  refId: string;
  description: string | null;
  source: DatasetSource;
  sourceConfiguration: PUSpecSheetSourceConfiguration;
  columns: DatasetColumn[];
};
