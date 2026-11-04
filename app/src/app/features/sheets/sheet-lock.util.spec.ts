import { SheetLockUtil } from './sheet-lock.util';

describe('otherUsersLock', () => {
  it('is the lock when someone else holds it', () => {
    const lock = { userId: 2, userName: 'A. Smith', isMine: false };

    expect(SheetLockUtil.otherUsersLock(lock)).toBe(lock);
  });

  it('is null when the item is free or the viewer holds the lock', () => {
    expect(SheetLockUtil.otherUsersLock(null)).toBeNull();
    expect(SheetLockUtil.otherUsersLock(undefined)).toBeNull();
    expect(SheetLockUtil.otherUsersLock({ userId: 1, userName: 'Me', isMine: true })).toBeNull();
  });
});
