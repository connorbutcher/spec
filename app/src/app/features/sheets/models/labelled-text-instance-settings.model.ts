import { LabelledTextSlot } from '../../templates/models/labelled-text-slot.model';

/**
 * The values one labelled text cell has on the sheet, when they differ from its template's: other labels,
 * more values or fewer. Null keeps the template's.
 */
export type LabelledTextInstanceSettings = {
  kind: 'LabelledText';
  slots: readonly LabelledTextSlot[] | null;
}
