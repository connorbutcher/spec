import { sheetUrl } from './sheet-url.util';

describe('sheetUrl', () => {
  const target = { phaseId: 3, sheetTypeId: 2 };

  it('reads the live sheet when no moment is chosen', () => {
    expect(sheetUrl(target, {})).toBe('/api/phases/3/sheets/2');
  });

  it('asks for a version by number', () => {
    expect(sheetUrl(target, { version: 4 })).toBe('/api/phases/3/sheets/2?version=4');
  });

  it('asks for a moment as an encoded date and time', () => {
    expect(sheetUrl(target, { asOf: '2026-09-12T10:00:00.000Z' })).toBe(
      '/api/phases/3/sheets/2?asOf=2026-09-12T10%3A00%3A00.000Z',
    );
  });
});
