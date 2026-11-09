import { CellConfiguration } from './models/cell-configuration';
import { LabelledTextSlot } from './models/labelled-text-slot.model';

/** The slots a cell has when neither its type nor the sheet names any. Matches the API's `LabelledTextRules.DefaultSlots`. */
const DEFAULT_SLOTS: readonly LabelledTextSlot[] = [{ key: 'value', label: '' }];

/** Lower-case letters and digits, starting with a letter. Matches the API. */
const KEY_PATTERN = /^[a-z][a-z0-9]{0,19}$/;

export class LabelledTextSlotsUtil {
  public static readonly mostSlots = 8;

  public static readonly longestLabel = 20;

  /** The template's slots for a cell: its configuration's, or one unlabelled value. */
  public static templateSlots(configuration: CellConfiguration | null): readonly LabelledTextSlot[] {
    return configuration?.kind === 'LabelledText' && configuration.slots ? configuration.slots : DEFAULT_SLOTS;
  }

  /** The slots a cell has on the sheet: the ones chosen for it there, or else its template's. */
  public static effective(
    configuration: CellConfiguration | null,
    chosen: readonly LabelledTextSlot[] | null | undefined,
  ): readonly LabelledTextSlot[] {
    return chosen ?? LabelledTextSlotsUtil.templateSlots(configuration);
  }

  /** The label colour a cell's labels are shown in, or null for the app's quiet text colour. */
  public static labelColor(configuration: CellConfiguration | null): string | null {
    return configuration?.kind === 'LabelledText' ? (configuration.labelColor ?? null) : null;
  }

  /** A key for a new slot, made from its label where it can be ("RH" → "rh"), unused by the other slots. */
  public static keyFor(label: string, slots: readonly LabelledTextSlot[]): string {
    const taken = new Set(slots.map((slot) => slot.key));
    const fromLabel = label.toLowerCase().replace(/[^a-z0-9]/g, '').replace(/^[0-9]+/, '').slice(0, 20);
    if (KEY_PATTERN.test(fromLabel) && !taken.has(fromLabel)) {
      return fromLabel;
    }
    let number = slots.length + 1;
    while (taken.has(`v${number}`)) {
      number++;
    }
    return `v${number}`;
  }

  /** Whether two lists of slots are the same, slot for slot and in order. */
  public static same(left: readonly LabelledTextSlot[] | null, right: readonly LabelledTextSlot[] | null): boolean {
    if (left === null || right === null) {
      return left === right;
    }
    return left.length === right.length && left.every((slot, index) => slot.key === right[index].key && slot.label === right[index].label);
  }
}
