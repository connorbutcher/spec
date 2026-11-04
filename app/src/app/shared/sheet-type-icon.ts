const ICONS_BY_NAME: Readonly<Record<string, string>> = {
  specification: 'pi-file',
  parts: 'pi-box',
  pfks: 'pi-key',
  'engine specifications': 'pi-cog',
  confirmation: 'pi-check-square',
  'torque sheet': 'pi-wrench',
};

export class SheetTypeIcon {
  /** The PrimeIcons class used for a sheet type, falling back to a generic document icon. */
  public static sheetTypeIcon(name: string): string {
    return ICONS_BY_NAME[name.trim().toLowerCase()] ?? 'pi-file';
  }
}
