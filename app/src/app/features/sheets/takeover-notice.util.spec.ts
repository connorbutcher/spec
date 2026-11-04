import { RowTakeover } from './models/row-takeover.model';
import { RowTakeoverStatus } from './models/row-takeover-status';
import { TakeoverNoticeUtil } from './takeover-notice.util';

class SpecHelpers {
  public static takeover(status: RowTakeoverStatus): RowTakeover {
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
}

const REQUESTER = 2;
const HOLDER = 1;
describe('takeoverNotice', () => {
  it.each<RowTakeoverStatus>(['Approved', 'GrantedOnTimeout', 'GrantedHolderAway'])(
    'tells the requester the row is theirs when it is %s',
    (status) => {
      const notice = TakeoverNoticeUtil.takeoverNotice(SpecHelpers.takeover(status), REQUESTER);

      expect(notice?.severity).toBe('success');
      expect(notice?.summary).toBe('Row checked out to you');
    },
  );

  it('tells the requester when the holder keeps the row', () => {
    expect(TakeoverNoticeUtil.takeoverNotice(SpecHelpers.takeover('Denied'), REQUESTER)?.detail).toBe(
      'Developer is keeping the row.',
    );
  });

  it('tells the requester when the holder changed the row first', () => {
    const notice = TakeoverNoticeUtil.takeoverNotice(SpecHelpers.takeover('KeptForChanges'), REQUESTER);

    expect(notice?.severity).toBe('warn');
    expect(notice?.detail).toBe(
      'Developer has changed the row, so it stays with them until they publish.',
    );
    expect(TakeoverNoticeUtil.takeoverNotice(SpecHelpers.takeover('KeptForChanges'), HOLDER)).toBeNull();
  });

  it('says nothing to the person who answered or withdrew', () => {
    expect(TakeoverNoticeUtil.takeoverNotice(SpecHelpers.takeover('Approved'), HOLDER)).toBeNull();
    expect(TakeoverNoticeUtil.takeoverNotice(SpecHelpers.takeover('Denied'), HOLDER)).toBeNull();
    expect(TakeoverNoticeUtil.takeoverNotice(SpecHelpers.takeover('Cancelled'), REQUESTER)).toBeNull();
  });

  it('keeps the notice up for a holder whose row was taken without their answer', () => {
    const notice = TakeoverNoticeUtil.takeoverNotice(SpecHelpers.takeover('GrantedOnTimeout'), HOLDER);

    expect(notice?.severity).toBe('warn');
    expect(notice?.sticky).toBe(true);
    expect(notice?.detail).toContain('Engineer Two took over a row you had checked out');
  });

  it('tells the holder when the request is withdrawn', () => {
    expect(TakeoverNoticeUtil.takeoverNotice(SpecHelpers.takeover('Cancelled'), HOLDER)?.summary).toBe('Request withdrawn');
  });

  it('says nothing to anyone else', () => {
    expect(TakeoverNoticeUtil.takeoverNotice(SpecHelpers.takeover('Approved'), 99)).toBeNull();
  });
});
