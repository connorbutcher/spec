type Container = Record<string, unknown> | unknown[];

/**
 * `next`, with every part that is deep-equal to the same part of `previous` swapped for the previous
 * object, so anything that watches a part by identity (a signal, an input, a `@for` item) only sees a
 * change where the data really changed. Returns `previous` itself when nothing differs.
 *
 * For plain data only (objects, arrays and primitives, as parsed from JSON). Neither argument is
 * changed. Parts of `next` that `isFresh` picks out are kept as they are, even if they are equal.
 */
export function reuseUnchanged<T>(
  previous: unknown,
  next: T,
  isFresh: (part: object) => boolean = () => false,
): T {
  if (Object.is(previous, next)) {
    return next;
  }
  if (!isContainer(previous) || !isContainer(next) || isFresh(next)) {
    return next;
  }
  if (Array.isArray(previous) !== Array.isArray(next)) {
    return next;
  }

  const before = previous as Record<string, unknown>;
  const after = next as Record<string, unknown>;
  const keys = Object.keys(after);
  const merged = (Array.isArray(next) ? [] : {}) as Record<string, unknown>;
  let unchanged = Object.keys(before).length === keys.length;
  for (const key of keys) {
    merged[key] = reuseUnchanged(before[key], after[key], isFresh);
    unchanged &&= key in before && Object.is(merged[key], before[key]);
  }
  return (unchanged ? previous : merged) as T;
}

function isContainer(value: unknown): value is Container {
  return typeof value === 'object' && value !== null;
}
