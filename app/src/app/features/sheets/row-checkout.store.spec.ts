import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { RowCheckoutStore } from './row-checkout.store';

describe('RowCheckoutStore', () => {
  let store: RowCheckoutStore;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        RowCheckoutStore,
        { provide: CurrentUserStore, useValue: { user: signal({ id: 1 }) } },
      ],
    });
    store = TestBed.inject(RowCheckoutStore);
  });

  it('starts with nobody in any row', () => {
    expect(store.locks().size).toBe(0);
  });

  it('turns each row someone is in into the lock it shows, marking the viewer’s own', () => {
    store.set([
      { rowId: 10, userId: 1, userName: 'Developer' },
      { rowId: 11, userId: 2, userName: 'Engineer Two' },
    ]);

    expect(store.locks().get(10)).toEqual({ userId: 1, userName: 'Developer', isMine: true });
    expect(store.locks().get(11)).toEqual({ userId: 2, userName: 'Engineer Two', isMine: false });
  });

  it('replaces the list each time the server sends one', () => {
    store.set([{ rowId: 10, userId: 2, userName: 'Engineer Two' }]);
    store.set([]);

    expect(store.locks().size).toBe(0);
  });
});
