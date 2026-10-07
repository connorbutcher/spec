import { RowTakeover } from './models/row-takeover.model';
import { RowTakeoverStatus } from './models/row-takeover-status';
import { takeoverNotice } from './takeover-notice.util';

const REQUESTER = 2;
const HOLDER = 1;

function takeover(status: RowTakeoverStatus): RowTakeover {
  return {
    id: 'a',
    sheetId: 5,
    rowId: 10,
    requesterUserId: REQUESTER,
    requesterName: 'Engineer Two',
    holderUserId: HOLDER,
    holderName: 'Developer',
    requestedAtUtc: '2026-10-07T12:00:00Z',
    expiresAtUtc: '2026-10-07T12:01:00Z',
    status,
  };
}

describe('takeoverNotice', () => {
  it.each<RowTakeoverStatus>(['Approved', 'GrantedOnTimeout', 'GrantedHolderAway'])(
    'tells the requester the row is theirs when it is %s',
    (status) => {
      const notice = takeoverNotice(takeover(status), REQUESTER);

      expect(notice?.severity).toBe('success');
      expect(notice?.summary).toBe('Row checked out to you');
    },
  );

  it('tells the requester when the holder keeps the row', () => {
    expect(takeoverNotice(takeover('Denied'), REQUESTER)?.detail).toBe(
      'Developer is keeping the row.',
    );
  });

  it('says nothing to the person who answered or withdrew', () => {
    expect(takeoverNotice(takeover('Approved'), HOLDER)).toBeNull();
    expect(takeoverNotice(takeover('Denied'), HOLDER)).toBeNull();
    expect(takeoverNotice(takeover('Cancelled'), REQUESTER)).toBeNull();
  });

  it('keeps the notice up for a holder whose row was taken without their answer', () => {
    const notice = takeoverNotice(takeover('GrantedOnTimeout'), HOLDER);

    expect(notice?.severity).toBe('warn');
    expect(notice?.sticky).toBe(true);
    expect(notice?.detail).toContain('Engineer Two took over a row you had checked out');
  });

  it('tells the holder when the request is withdrawn', () => {
    expect(takeoverNotice(takeover('Cancelled'), HOLDER)?.summary).toBe('Request withdrawn');
  });

  it('says nothing to anyone else', () => {
    expect(takeoverNotice(takeover('Approved'), 99)).toBeNull();
  });
});
