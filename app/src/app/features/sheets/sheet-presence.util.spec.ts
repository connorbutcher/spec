import { SheetPresenceUser } from './models/sheet-presence-user.model';
import {
  checkedOutRowCounts,
  initials,
  presenceDescription,
  viewerFirst,
} from './sheet-presence.util';
import { fixtureRow } from './sheet-structure.fixture';

function user(userId: number, displayName: string, connectionCount = 1): SheetPresenceUser {
  return { userId, displayName, connectionCount };
}

describe('viewerFirst', () => {
  it('moves the viewer to the front and keeps everyone else in order', () => {
    const users = [user(3, 'Alex'), user(1, 'Developer'), user(2, 'Sam')];

    expect(viewerFirst(users, 1).map((each) => each.userId)).toEqual([1, 3, 2]);
  });

  it('changes nothing when the viewer is not known yet', () => {
    const users = [user(3, 'Alex'), user(2, 'Sam')];

    expect(viewerFirst(users, null)).toEqual(users);
  });
});

describe('checkedOutRowCounts', () => {
  it('counts the rows checked out to each person, and leaves free rows out', () => {
    const mine = { ...fixtureRow([]), lock: { userId: 1, userName: 'Developer', isMine: true } };
    const theirs = { ...fixtureRow([]), lock: { userId: 2, userName: 'Sam', isMine: false } };
    const alsoTheirs = { ...fixtureRow([]), lock: { userId: 2, userName: 'Sam', isMine: false } };

    const counts = checkedOutRowCounts([mine, theirs, fixtureRow([]), alsoTheirs]);

    expect([...counts]).toEqual([
      [1, 1],
      [2, 2],
    ]);
  });
});

describe('initials', () => {
  it('takes the first letters of the first two words', () => {
    expect(initials('Engineer Two')).toBe('ET');
    expect(initials('developer')).toBe('D');
    expect(initials('  Anna  de la Cruz ')).toBe('AD');
    expect(initials('')).toBe('');
  });
});

describe('presenceDescription', () => {
  it('is just the name for someone with nothing checked out', () => {
    expect(presenceDescription(user(2, 'Sam'), false, 0)).toBe('Sam');
  });

  it('marks the viewer, and adds rows and tabs when there are any', () => {
    expect(presenceDescription(user(1, 'Developer', 3), true, 1)).toBe(
      'Developer (you) · 1 row checked out · 3 tabs',
    );
    expect(presenceDescription(user(2, 'Sam'), false, 2)).toBe('Sam · 2 rows checked out');
  });
});
