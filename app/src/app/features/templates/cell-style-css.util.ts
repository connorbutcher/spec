import { CellStyle } from './models/cell-style.model';

const ALIGNMENT = { Left: 'left', Center: 'center', Right: 'right' } as const;

/**
 * Inline CSS for a cell's effective style, for `[style]` bindings. Unset values are left out, so the
 * surrounding styles apply.
 */
export function cellStyleCss(style: CellStyle): Record<string, string> {
  const css: Record<string, string> = {};
  if (style.bold !== null && style.bold !== undefined) {
    css['font-weight'] = style.bold ? '600' : '400';
  }
  if (style.italic !== null && style.italic !== undefined) {
    css['font-style'] = style.italic ? 'italic' : 'normal';
  }
  if (style.align) {
    css['text-align'] = ALIGNMENT[style.align];
  }
  if (style.textColor) {
    css['color'] = style.textColor;
  }
  if (style.backgroundColor) {
    css['background-color'] = style.backgroundColor;
  }
  return css;
}
