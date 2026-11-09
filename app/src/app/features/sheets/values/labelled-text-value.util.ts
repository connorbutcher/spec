import { CellConfiguration } from '../../templates/models/cell-configuration';
import { LabelledTextSlot } from '../../templates/models/labelled-text-slot.model';
import { LabelledTextSlotsUtil } from '../../templates/labelled-text-slots.util';
import { SheetCell } from '../models/sheet-cell.model';

export class LabelledTextValueUtil {
  /** The values a labelled text cell has on the sheet: the ones chosen for it there, or else its template's. */
  public static slotsOf(cell: SheetCell, configuration: CellConfiguration | null): readonly LabelledTextSlot[] {
    const settings = cell.settings;
    const chosen = settings?.kind === 'LabelledText' ? settings.slots : null;
    return LabelledTextSlotsUtil.effective(configuration, chosen);
  }

  /** The values to save: trimmed, without the empty ones, in slot order; null when none are left. */
  public static cleaned(slots: readonly LabelledTextSlot[], values: Readonly<Record<string, string>>): Record<string, string> | null {
    const cleaned: Record<string, string> = {};
    for (const slot of slots) {
      const text = values[slot.key]?.trim();
      if (text) {
        cleaned[slot.key] = text;
      }
    }
    return Object.keys(cleaned).length > 0 ? cleaned : null;
  }

  /** Whether two sets of values hold the same thing in every slot. */
  public static same(left: Readonly<Record<string, string>> | null, right: Readonly<Record<string, string>> | null): boolean {
    const leftEntries = Object.entries(left ?? {});
    const rightValues = right ?? {};
    return leftEntries.length === Object.keys(rightValues).length
      && leftEntries.every(([key, value]) => rightValues[key] === value);
  }
}
