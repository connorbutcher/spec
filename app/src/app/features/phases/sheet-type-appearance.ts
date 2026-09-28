import { SheetTypeAppearance } from './sheet-type-appearance.model';

const APPEARANCE_BY_NAME: Readonly<Record<string, SheetTypeAppearance>> = {
  specification: { icon: 'pi-file', accent: '#2f59a8' },
  parts: { icon: 'pi-box', accent: '#0f766e' },
  pfks: { icon: 'pi-key', accent: '#b45309' },
  'engine specifications': { icon: 'pi-cog', accent: '#6d28d9' },
  confirmation: { icon: 'pi-check-square', accent: '#15803d' },
  'torque sheet': { icon: 'pi-wrench', accent: '#be123c' },
};

const DEFAULT_APPEARANCE: SheetTypeAppearance = { icon: 'pi-file', accent: '#475569' };

/** The icon and accent colour used for a sheet type, with a neutral fallback for new types. */
export function sheetTypeAppearance(name: string): SheetTypeAppearance {
  return APPEARANCE_BY_NAME[name.trim().toLowerCase()] ?? DEFAULT_APPEARANCE;
}
