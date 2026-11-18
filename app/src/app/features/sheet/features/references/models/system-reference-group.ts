import { SheetExternalReference } from '../../../../../core/models/domain/sheets/sheet';

export type SystemReferenceGroup = {
  group: string;
  references: SheetExternalReference[];
};
