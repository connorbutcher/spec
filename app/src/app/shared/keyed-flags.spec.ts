import { KeyedFlags } from './keyed-flags';

describe('KeyedFlags', () => {
  it('turns one key on at a time', () => {
    const flags = new KeyedFlags();
    const first = flags.flag(1);
    const second = flags.flag(2);

    flags.set(1);
    expect([first(), second()]).toEqual([true, false]);

    flags.set(2);
    expect([first(), second()]).toEqual([false, true]);

    flags.set(null);
    expect([first(), second()]).toEqual([false, false]);
  });

  it('gives a flag asked for later the right answer', () => {
    const flags = new KeyedFlags();
    flags.set(3);

    expect(flags.flag(3)()).toBe(true);
    expect(flags.flag(4)()).toBe(false);
  });
});
