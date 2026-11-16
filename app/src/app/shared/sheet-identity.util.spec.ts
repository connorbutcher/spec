import { SheetIdentityUtil } from './sheet-identity.util';

const sheetType = {
  identifiers: [
    { id: 1, key: 'serialNumber', name: 'Serial number', kind: 'Text' as const, isRequired: true, displayOrder: 1, options: [] },
    { id: 2, key: 'build', name: 'Build', kind: 'Option' as const, isRequired: false, displayOrder: 2, options: ['A'] },
  ],
};

describe('SheetIdentityUtil', () => {
  it('names a sheet by its values in the sheet type order', () => {
    const sheet = { identifiers: [{ identifierId: 2, value: 'A' }, { identifierId: 1, value: 'ESN-1' }] };

    expect(SheetIdentityUtil.label(sheet, sheetType)).toBe('ESN-1 · A');
  });

  it('leaves out identifiers the sheet has no value for', () => {
    expect(SheetIdentityUtil.label({ identifiers: [{ identifierId: 1, value: 'ESN-1' }] }, sheetType)).toBe('ESN-1');
  });

  it('says when a sheet has no identifiers', () => {
    expect(SheetIdentityUtil.label({ identifiers: [] }, sheetType)).toBe('No identifiers');
  });

  it('puts approval before drafts, and drafts before publishing', () => {
    expect(SheetIdentityUtil.status({ pendingSubmissionCount: 1, draftCount: 3, latestVersionNumber: 2 })).toBe('Awaiting approval');
    expect(SheetIdentityUtil.status({ pendingSubmissionCount: 0, draftCount: 3, latestVersionNumber: 2 })).toBe('3 drafts');
    expect(SheetIdentityUtil.status({ pendingSubmissionCount: 0, draftCount: 0, latestVersionNumber: null })).toBe('Not published');
    expect(SheetIdentityUtil.status({ pendingSubmissionCount: 0, draftCount: 0, latestVersionNumber: 2 })).toBe('Published');
  });
});
