import { User } from '../access/user';
import { SpecSheetPhase } from '../types/spec-sheet-phase';
import { SpecSheetType } from '../types/spec-sheet-type';
import { SpecSheetPropertyValueSummary } from './properties/spec-sheet-property-value-summary';

export type SpecSheetSummary = {
  id: number;
  specSheetPhaseId: number;
  specSheetTypeId: number;
  isPublished: boolean;
  createdByUserId: number;
  createdDateTime: string;
  createdByUser: User;
  hasBeenMigrated: boolean;
  publishedByUserId?: number;
  publishedDateTime?: string;
  publishedByUser?: User;
  changeLog?: string;
  specSheetPhase: SpecSheetPhase;
  specSheetType: SpecSheetType;
  propertyValues: SpecSheetPropertyValueSummary[];
};
