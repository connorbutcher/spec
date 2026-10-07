import { SheetDraftSummary } from './models/sheet-draft-summary.model';
import {
  effectivePublishScope,
  otherDraftCount,
  otherDraftsLabel,
  publishScopeOptions,
} from './publish-scope.util';

const asha: SheetDraftSummary = { userId: 2, userName: 'Asha', draftCount: 3 };
const ben: SheetDraftSummary = { userId: 3, userName: 'Ben', draftCount: 1 };
const cara: SheetDraftSummary = { userId: 4, userName: 'Cara', draftCount: 2 };

describe('publish scope', () => {
  it("adds up other people's changes", () => {
    expect(otherDraftCount([])).toBe(0);
    expect(otherDraftCount([asha, ben])).toBe(4);
  });

  it('names whose changes they are', () => {
    expect(otherDraftsLabel([asha])).toBe('3 by Asha');
    expect(otherDraftsLabel([asha, ben])).toBe('3 by Asha and 1 by Ben');
    expect(otherDraftsLabel([asha, ben, cara])).toBe('3 by Asha, 1 by Ben and 2 by Cara');
  });

  it('counts each choice: my own, and mine with everyone else’s', () => {
    const [mine, all] = publishScopeOptions(2, [asha, ben]);

    expect(mine).toEqual(expect.objectContaining({ scope: 'Mine', count: 2, disabled: false }));
    expect(all).toEqual(expect.objectContaining({ scope: 'All', count: 6, disabled: false }));
    expect(all.detail).toContain('3 by Asha and 1 by Ben');
  });

  it('defaults to my own changes', () => {
    expect(effectivePublishScope(publishScopeOptions(2, [asha]), null)).toBe('Mine');
  });

  it('keeps the choice of everything while other people have changes', () => {
    expect(effectivePublishScope(publishScopeOptions(2, [asha]), 'All')).toBe('All');
  });

  it('goes back to my own changes once nobody else has any', () => {
    const options = publishScopeOptions(2, []);

    expect(options[1].disabled).toBe(true);
    expect(effectivePublishScope(options, 'All')).toBe('Mine');
  });

  it('takes everything when I have changed nothing myself', () => {
    const options = publishScopeOptions(0, [asha]);

    expect(options[0].disabled).toBe(true);
    expect(effectivePublishScope(options, null)).toBe('All');
    expect(effectivePublishScope(options, 'Mine')).toBe('All');
  });
});
