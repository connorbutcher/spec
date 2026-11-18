import { SheetExternalReference } from '../../../../../core/models/domain/sheets/sheet-external-reference';

export type SystemReferencesConfirmationDialogData = {
  type: string;
  references: SheetExternalReference[];
};
