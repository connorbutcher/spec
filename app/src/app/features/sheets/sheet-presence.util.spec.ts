import { SheetPresenceUser } from './models/sheet-presence-user.model';
import {
  SheetPresenceUtil } from './sheet-presence.util';
import { SheetStructureFixture } from './sheet-structure.fixture';

class SpecHelpers {
  public static user(userId: number, displayName: string, connectionCount = 1): SheetPresenceUser {
    return { userId, displayName, connectionCount };
  }
}
describe('viewerFirst', () => {
  it('moves the viewer to the front and keeps everyone else in order', () => {
    const users = [SpecHelpers.user(3, 'Alex'), SpecHelpers.user(1, 'Developer'), SpecHelpers.user(2, 'Sam')];

    expect(SheetPresenceUtil.viewerFirst(users, 1).map((each) => each.userId)).toEqual([1, 3, 2]);
  });

  it('changes nothing when the viewer is not known yet', () => {
    const users = [SpecHelpers.user(3, 'Alex'), SpecHelpers.user(2, 'Sam')];

    expect(SheetPresenceUtil.viewerFirst(users, null)).toEqual(users);
  });
});

describe('checkedOutRowCounts', () => {
  it('counts the rows checked out to each person, and leaves free rows out', () => {
    const mine = { ...SheetStructureFixture.fixtureRow([]), lock: { userId: 1, userName: 'Developer', isMine: true } };
    const theirs = { ...SheetStructureFixture.fixtureRow([]), lock: { userId: 2, userName: 'Sam', isMine: false } };
    const alsoTheirs = { ...SheetStructureFixture.fixtureRow([]), lock: { userId: 2, userName: 'Sam', isMine: false } };

    const counts = SheetPresenceUtil.checkedOutRowCounts([mine, theirs, SheetStructureFixture.fixtureRow([]), alsoTheirs]);

    expect([...counts]).toEqual([
      [1, 1],
      [2, 2],
    ]);
  });
});

describe('initials', () => {
  it('takes the first letters of the first two words', () => {
    expect(SheetPresenceUtil.initials('Engineer Two')).toBe('ET');
    expect(SheetPresenceUtil.initials('developer')).toBe('D');
    expect(SheetPresenceUtil.initials('  Anna  de la Cruz ')).toBe('AD');
    expect(SheetPresenceUtil.initials('')).toBe('');
  });
});

describe('presenceDescription', () => {
  it('is just the name for someone with nothing checked out', () => {
    expect(SheetPresenceUtil.presenceDescription(SpecHelpers.user(2, 'Sam'), false, 0)).toBe('Sam');
  });

  it('marks the viewer, and adds rows and tabs when there are any', () => {
    expect(SheetPresenceUtil.presenceDescription(SpecHelpers.user(1, 'Developer', 3), true, 1)).toBe(
      'Developer (you) · 1 row checked out · 3 tabs',
    );
    expect(SheetPresenceUtil.presenceDescription(SpecHelpers.user(2, 'Sam'), false, 2)).toBe('Sam · 2 rows checked out');
  });
});
