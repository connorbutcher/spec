import { SheetUrlUtil } from './sheet-url.util';

describe('sheetTarget', () => {
  it('reads both ids from the route', () => {
    expect(SheetUrlUtil.sheetTarget('3', '2')).toEqual({ phaseId: 3, sheetTypeId: 2, sheetId: null });
  });

  it('is nothing when an id is missing or is not a whole number above zero', () => {
    expect(SheetUrlUtil.sheetTarget(null, '2')).toBeNull();
    expect(SheetUrlUtil.sheetTarget('abc', '2')).toBeNull();
    expect(SheetUrlUtil.sheetTarget('3', '0')).toBeNull();
    expect(SheetUrlUtil.sheetTarget('3', '1.5')).toBeNull();
  });

  it('reads the sheet id when the route names one', () => {
    expect(SheetUrlUtil.sheetTarget('3', '2', '17')).toEqual({ phaseId: 3, sheetTypeId: 2, sheetId: 17 });
    expect(SheetUrlUtil.sheetTarget('3', '2', 'x')).toBeNull();
  });
});

describe('sheetUrl', () => {
  const target = { phaseId: 3, sheetTypeId: 2, sheetId: null };
  const named = { phaseId: 3, sheetTypeId: 2, sheetId: 17 };

  it('opens the one sheet of a type in a phase live, by phase and type', () => {
    expect(SheetUrlUtil.sheetUrl(target, {})).toBe('/api/phases/3/sheets/2');
  });

  it('reads a sheet named by id live from its own address', () => {
    expect(SheetUrlUtil.sheetUrl(named, {})).toBe('/api/sheets/17');
  });

  it('asks for a version by number, by sheet id', () => {
    expect(SheetUrlUtil.sheetUrl(named, { version: 4 })).toBe('/api/sheets/17/versions/4');
    expect(SheetUrlUtil.sheetUrl(target, { version: 4 }, 17)).toBe('/api/sheets/17/versions/4');
  });

  it('asks for a moment as an encoded date and time, by sheet id', () => {
    expect(SheetUrlUtil.sheetUrl(target, { asOf: '2026-09-12T10:00:00.000Z' }, 17)).toBe(
      '/api/sheets/17/versions/at/2026-09-12T10%3A00%3A00.000Z',
    );
  });

  it('has no address for a past view until the id of the sheet is known', () => {
    expect(SheetUrlUtil.sheetUrl(target, { version: 4 })).toBeUndefined();
  });
});

describe('reviewing a submission', () => {
  it('asks for the sheet of the submission with its changes on top', () => {
    expect(SheetUrlUtil.sheetUrl({ phaseId: 3, sheetTypeId: 2, sheetId: null }, { submission: 12 })).toBe('/api/submissions/12/sheet');
  });

  it('reads the submission from the review parameter, if it is an id', () => {
    expect(SheetUrlUtil.reviewedSubmission('12')).toBe(12);
    expect(SheetUrlUtil.reviewedSubmission(null)).toBeNull();
    expect(SheetUrlUtil.reviewedSubmission('x')).toBeNull();
    expect(SheetUrlUtil.reviewedSubmission('0')).toBeNull();
  });
});
