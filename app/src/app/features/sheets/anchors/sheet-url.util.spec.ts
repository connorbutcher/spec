import { SheetUrlUtil } from './sheet-url.util';

describe('sheetTarget', () => {
  it('reads both ids from the route', () => {
    expect(SheetUrlUtil.sheetTarget('3', '2')).toEqual({ phaseId: 3, sheetTypeId: 2 });
  });

  it('is nothing when an id is missing or is not a whole number above zero', () => {
    expect(SheetUrlUtil.sheetTarget(null, '2')).toBeNull();
    expect(SheetUrlUtil.sheetTarget('abc', '2')).toBeNull();
    expect(SheetUrlUtil.sheetTarget('3', '0')).toBeNull();
    expect(SheetUrlUtil.sheetTarget('3', '1.5')).toBeNull();
  });
});

describe('sheetUrl', () => {
  const target = { phaseId: 3, sheetTypeId: 2 };

  it('reads the live sheet when no moment is chosen', () => {
    expect(SheetUrlUtil.sheetUrl(target, {})).toBe('/api/phases/3/sheets/2');
  });

  it('asks for a version by number', () => {
    expect(SheetUrlUtil.sheetUrl(target, { version: 4 })).toBe('/api/phases/3/sheets/2?version=4');
  });

  it('asks for a moment as an encoded date and time', () => {
    expect(SheetUrlUtil.sheetUrl(target, { asOf: '2026-09-12T10:00:00.000Z' })).toBe(
      '/api/phases/3/sheets/2?asOf=2026-09-12T10%3A00%3A00.000Z',
    );
  });
});

describe('reviewing a submission', () => {
  it('asks for the live sheet with the submission on top', () => {
    expect(SheetUrlUtil.sheetUrl({ phaseId: 3, sheetTypeId: 2 }, { submission: 12 })).toBe(
      '/api/phases/3/sheets/2?submission=12',
    );
  });

  it('reads the submission from the review parameter, if it is an id', () => {
    expect(SheetUrlUtil.reviewedSubmission('12')).toBe(12);
    expect(SheetUrlUtil.reviewedSubmission(null)).toBeNull();
    expect(SheetUrlUtil.reviewedSubmission('x')).toBeNull();
    expect(SheetUrlUtil.reviewedSubmission('0')).toBeNull();
  });
});
