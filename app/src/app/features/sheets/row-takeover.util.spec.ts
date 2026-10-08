import { fixtureTakeover } from './row-takeover.fixture';
import { secondsUntilGranted, withoutTakeover, withTakeover } from './row-takeover.util';

describe('secondsUntilGranted', () => {
  it('rounds up to whole seconds, and stops at zero', () => {
    const takeover = fixtureTakeover();

    expect(secondsUntilGranted(takeover, Date.parse('2026-10-07T12:00:18.200Z'))).toBe(42);
    expect(secondsUntilGranted(takeover, Date.parse('2026-10-07T12:01:00Z'))).toBe(0);
    expect(secondsUntilGranted(takeover, Date.parse('2026-10-07T12:02:00Z'))).toBe(0);
  });
});

describe('withTakeover', () => {
  it('adds a new request to the end', () => {
    const first = fixtureTakeover({ id: 'a' });
    const second = fixtureTakeover({ id: 'b' });

    expect(withTakeover([first], second)).toEqual([first, second]);
  });

  it('replaces the copy of a request that is already listed', () => {
    const first = fixtureTakeover({ id: 'a' });
    const again = fixtureTakeover({ id: 'a', expiresAtUtc: '2026-10-07T12:05:00Z' });

    expect(withTakeover([first], again)).toEqual([again]);
  });
});

describe('withoutTakeover', () => {
  it('takes a request off the list by its id', () => {
    const first = fixtureTakeover({ id: 'a' });
    const second = fixtureTakeover({ id: 'b' });

    expect(withoutTakeover([first, second], 'a')).toEqual([second]);
    expect(withoutTakeover([first], 'missing')).toEqual([first]);
  });
});
