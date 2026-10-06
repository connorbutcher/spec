import { reuseUnchanged } from './reuse-unchanged.util';

describe('reuseUnchanged', () => {
  it('returns the previous object when nothing differs', () => {
    const previous = { id: 1, rows: [{ id: 2, cells: [{ id: 3, value: 'a' }] }] };
    const next = structuredClone(previous);

    expect(reuseUnchanged(previous, next)).toBe(previous);
  });

  it('keeps unchanged parts and replaces only the path to a change', () => {
    const previous = {
      rows: [
        { id: 1, cells: [{ id: 10, value: 'a' }] },
        { id: 2, cells: [{ id: 20, value: 'b' }] },
      ],
    };
    const next = structuredClone(previous);
    next.rows[1].cells[0].value = 'changed';

    const result = reuseUnchanged(previous, next);

    expect(result).toEqual(next);
    expect(result).not.toBe(previous);
    expect(result.rows[0]).toBe(previous.rows[0]);
    expect(result.rows[1]).not.toBe(previous.rows[1]);
  });

  it('does not change either argument', () => {
    const previous = { rows: [{ id: 1 }, { id: 2 }] };
    const next = { rows: [{ id: 1 }, { id: 3 }] };
    const nextRows = next.rows;

    reuseUnchanged(previous, next);

    expect(previous).toEqual({ rows: [{ id: 1 }, { id: 2 }] });
    expect(next.rows).toBe(nextRows);
    expect(next.rows[0]).not.toBe(previous.rows[0]);
  });

  it('sees added, removed and reordered items as changes', () => {
    const previous = { rows: [{ id: 1 }, { id: 2 }] };

    expect(reuseUnchanged(previous, { rows: [{ id: 1 }] }).rows).toEqual([{ id: 1 }]);
    expect(reuseUnchanged(previous, { rows: [{ id: 1 }, { id: 2 }, { id: 3 }] }).rows).toHaveLength(
      3,
    );
    expect(reuseUnchanged(previous, { rows: [{ id: 2 }, { id: 1 }] }).rows).toEqual([
      { id: 2 },
      { id: 1 },
    ]);
  });

  it('tells a missing key from an undefined one, and null from an object', () => {
    const previous: Record<string, unknown> = { a: undefined, lock: null };
    const renamed: Record<string, unknown> = { b: undefined, lock: null };
    const locked: Record<string, unknown> = { a: undefined, lock: { isMine: true } };

    expect(Object.keys(reuseUnchanged(previous, renamed))).toEqual(['b', 'lock']);
    expect(reuseUnchanged(previous, locked)).toEqual(locked);
    expect(reuseUnchanged(locked, { a: undefined, lock: null })).toEqual(previous);
  });

  it('does not mistake an array for an object with the same keys', () => {
    const next = { 0: 'a' };

    expect(reuseUnchanged(['a'], next)).toBe(next);
  });

  it('returns the next value when there is nothing to compare it with', () => {
    const next = { id: 1 };

    expect(reuseUnchanged(undefined, next)).toBe(next);
    expect(reuseUnchanged(null, next)).toBe(next);
  });

  it('keeps the parts marked as fresh, and so the path to them, even when they are equal', () => {
    const previous = { rows: [{ id: 1 }, { id: 2 }] };
    const next = structuredClone(previous);
    const fresh = next.rows[1];

    const result = reuseUnchanged(previous, next, (part) => part === fresh);

    expect(result).not.toBe(previous);
    expect(result.rows[0]).toBe(previous.rows[0]);
    expect(result.rows[1]).toBe(fresh);
  });
});
