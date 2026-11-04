import { RowTakeoverFixture } from './row-takeover.fixture';
import { RowTakeoverUtil } from './row-takeover.util';

describe('secondsUntilGranted', () => {
  it('rounds up to whole seconds, and stops at zero', () => {
    const takeover = RowTakeoverFixture.fixtureTakeover();

    expect(RowTakeoverUtil.secondsUntilGranted(takeover, Date.parse('2026-10-07T12:00:18.200Z'))).toBe(42);
    expect(RowTakeoverUtil.secondsUntilGranted(takeover, Date.parse('2026-10-07T12:01:00Z'))).toBe(0);
    expect(RowTakeoverUtil.secondsUntilGranted(takeover, Date.parse('2026-10-07T12:02:00Z'))).toBe(0);
  });
});

describe('withTakeover', () => {
  it('adds a new request to the end', () => {
    const first = RowTakeoverFixture.fixtureTakeover({ id: 'a' });
    const second = RowTakeoverFixture.fixtureTakeover({ id: 'b' });

    expect(RowTakeoverUtil.withTakeover([first], second)).toEqual([first, second]);
  });

  it('replaces the copy of a request that is already listed', () => {
    const first = RowTakeoverFixture.fixtureTakeover({ id: 'a' });
    const again = RowTakeoverFixture.fixtureTakeover({ id: 'a', expiresAtUtc: '2026-10-07T12:05:00Z' });

    expect(RowTakeoverUtil.withTakeover([first], again)).toEqual([again]);
  });
});

describe('withoutTakeover', () => {
  it('takes a request off the list by its id', () => {
    const first = RowTakeoverFixture.fixtureTakeover({ id: 'a' });
    const second = RowTakeoverFixture.fixtureTakeover({ id: 'b' });

    expect(RowTakeoverUtil.withoutTakeover([first, second], 'a')).toEqual([second]);
    expect(RowTakeoverUtil.withoutTakeover([first], 'missing')).toEqual([first]);
  });
});
