import { SpecSheetPropertyValueSummary } from '../../../core/models/domain/spec-sheet/properties/spec-sheet-property-value-summary';
import { SpecSheetSummary } from '../../../core/models/domain/spec-sheet/spec-sheet-summary';

export type SpecSheetTreeNodeData = {
  property: SpecSheetPropertyValueSummary;
  specSheets: SpecSheetSummary[];
};
