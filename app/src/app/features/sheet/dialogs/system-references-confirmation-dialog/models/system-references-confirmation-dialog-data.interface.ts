import { SpecSheetExternalReference } from '../../../../../core/models/domain/references/spec-sheet-external-reference';

export type SystemReferencesConfirmationDialogData = {
  type: string;
  references: SpecSheetExternalReference[];
};
