import {
  applyOverrides,
  effectiveConfiguration,
  hasSetting,
  withSetting,
} from './cell-settings.util';
import { cellStyleCss } from './cell-style-css.util';
import { CellConfiguration } from './models/cell-configuration';
import { CellStyle } from './models/cell-style.model';

describe('applyOverrides', () => {
  it('uses override values and keeps the rest', () => {
    const defaults: CellStyle = { bold: true, align: 'Left', textColor: '#111111' };
    const effective = applyOverrides(defaults, { align: 'Right', textColor: null });

    expect(effective).toEqual({ bold: true, align: 'Right', textColor: '#111111' });
  });

  it('returns the defaults when there are no overrides', () => {
    expect(applyOverrides({ bold: true }, null)).toEqual({ bold: true });
  });
});

describe('effectiveConfiguration', () => {
  const defaults: CellConfiguration = { kind: 'Number', decimalPlaces: 2, unit: 'Nm' };

  it('lays a same-kind override on the defaults', () => {
    const override: CellConfiguration = { kind: 'Number', maxValue: 50 };

    expect(effectiveConfiguration(defaults, override)).toEqual({
      kind: 'Number',
      decimalPlaces: 2,
      unit: 'Nm',
      maxValue: 50,
    });
  });

  it('ignores an override for another kind', () => {
    expect(effectiveConfiguration(defaults, { kind: 'Text', maxLength: 5 })).toBe(defaults);
  });
});

describe('withSetting and hasSetting', () => {
  it('clears a setting with null', () => {
    const cleared = withSetting({ kind: 'Text', maxLength: 10 }, 'maxLength', null);

    expect(hasSetting(cleared, 'maxLength')).toBe(false);
    expect(hasSetting({ multiline: false }, 'multiline')).toBe(true);
  });
});

describe('cellStyleCss', () => {
  it('maps set values to CSS and leaves unset ones out', () => {
    expect(
      cellStyleCss({ bold: true, align: 'Center', backgroundColor: '#f1f5f9', italic: null }),
    ).toEqual({
      'font-weight': '600',
      'text-align': 'center',
      'background-color': '#f1f5f9',
    });
  });
});
