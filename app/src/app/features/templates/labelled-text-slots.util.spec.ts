import { LabelledTextSlotsUtil } from './labelled-text-slots.util';
import { CellConfiguration } from './models/cell-configuration';

const HANDS: CellConfiguration = {
  kind: 'LabelledText',
  slots: [
    { key: 'rh', label: 'RH' },
    { key: 'lh', label: 'LH' },
  ],
  labelColor: '#dc2626',
};

describe('LabelledTextSlotsUtil', () => {
  it('uses the slots chosen on the sheet before the template', () => {
    const chosen = [{ key: 'both', label: 'Both' }];

    expect(LabelledTextSlotsUtil.effective(HANDS, chosen)).toBe(chosen);
    expect(LabelledTextSlotsUtil.effective(HANDS, null).map((slot) => slot.key)).toEqual(['rh', 'lh']);
  });

  it('gives a cell with no slots named one unlabelled value', () => {
    expect(LabelledTextSlotsUtil.templateSlots({ kind: 'LabelledText' })).toEqual([{ key: 'value', label: '' }]);
  });

  it('makes a new key from the label, unless it is taken or unusable', () => {
    const slots = [{ key: 'rh', label: 'RH' }];

    expect(LabelledTextSlotsUtil.keyFor('LH', slots)).toBe('lh');
    expect(LabelledTextSlotsUtil.keyFor('R.H.', slots)).toBe('v2');
    expect(LabelledTextSlotsUtil.keyFor('', slots)).toBe('v2');
  });

  it('compares slots in order', () => {
    const swapped = [
      { key: 'lh', label: 'LH' },
      { key: 'rh', label: 'RH' },
    ];

    expect(LabelledTextSlotsUtil.same(LabelledTextSlotsUtil.templateSlots(HANDS), swapped)).toBe(false);
    expect(LabelledTextSlotsUtil.same(swapped, [...swapped])).toBe(true);
  });
});
