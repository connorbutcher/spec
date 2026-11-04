export class TemplateNamesUtil {
  /** `base`, or `base 2`, `base 3`... whichever isn't taken. With `numbered`, starts at `base 1`. */
  public static nextName(base: string, taken: string[], numbered = false): string {
    const names = new Set(taken.map((name) => name.toLowerCase()));
    if (!numbered && !names.has(base.toLowerCase())) {
      return base;
    }
    for (let number = numbered ? 1 : 2; ; number++) {
      const candidate = `${base} ${number}`;
      if (!names.has(candidate.toLowerCase())) {
        return candidate;
      }
    }
  }
}
