import { otherUsersLock } from './sheet-lock.util';

describe('otherUsersLock', () => {
  it('is the lock when someone else holds it', () => {
    const lock = { userId: 2, userName: 'A. Smith', isMine: false };

    expect(otherUsersLock(lock)).toBe(lock);
  });

  it('is null when the item is free or the viewer holds the lock', () => {
    expect(otherUsersLock(null)).toBeNull();
    expect(otherUsersLock(undefined)).toBeNull();
    expect(otherUsersLock({ userId: 1, userName: 'Me', isMine: true })).toBeNull();
  });
});
