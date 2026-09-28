const ICONS_BY_NAME: Readonly<Record<string, string>> = {
  specification: 'pi-file',
  parts: 'pi-box',
  pfks: 'pi-key',
  'engine specifications': 'pi-cog',
  confirmation: 'pi-check-square',
  'torque sheet': 'pi-wrench',
};

/** The PrimeIcons class used for a sheet type, falling back to a generic document icon. */
export function sheetTypeIcon(name: string): string {
  return ICONS_BY_NAME[name.trim().toLowerCase()] ?? 'pi-file';
}
