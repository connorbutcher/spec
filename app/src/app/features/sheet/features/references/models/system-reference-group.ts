import { SpecSheetExternalReference } from '../../../../../core/models/domain/references/spec-sheet-external-reference';

export type SystemReferenceGroup = {
  group: string;
  references: SpecSheetExternalReference[];
};
