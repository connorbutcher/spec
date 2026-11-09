import { LabelledTextValueUtil } from './labelled-text-value.util';

const SLOTS = [
  { key: 'rh', label: 'RH' },
  { key: 'lh', label: 'LH' },
];

describe('LabelledTextValueUtil', () => {
  it('saves the values trimmed, in slot order, leaving out empty ones and other slots', () => {
    const cleaned = LabelledTextValueUtil.cleaned(SLOTS, { lh: ' P-1003-L ', rh: '', old: 'P-0999' });

    expect(cleaned).toEqual({ lh: 'P-1003-L' });
  });

  it('saves nothing when every value is empty', () => {
    expect(LabelledTextValueUtil.cleaned(SLOTS, { rh: ' ' })).toBeNull();
  });

  it('compares values slot by slot', () => {
    expect(LabelledTextValueUtil.same({ rh: 'A', lh: 'B' }, { lh: 'B', rh: 'A' })).toBe(true);
    expect(LabelledTextValueUtil.same({ rh: 'A' }, { rh: 'A', lh: 'B' })).toBe(false);
    expect(LabelledTextValueUtil.same(null, {})).toBe(true);
  });
});
