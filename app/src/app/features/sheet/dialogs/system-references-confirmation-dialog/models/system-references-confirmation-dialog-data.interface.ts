import { SheetExternalReference } from '../../../../../core/models/domain/sheets/sheet';

export type SystemReferencesConfirmationDialogData = {
  type: string;
  references: SheetExternalReference[];
};
