import { SheetExternalReference } from '../../../../../core/models/domain/sheets/sheet-external-reference';

export type SystemReferenceGroup = {
  group: string;
  references: SheetExternalReference[];
};
