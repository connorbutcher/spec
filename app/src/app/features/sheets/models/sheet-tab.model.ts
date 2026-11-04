/** One tab of the sheet switcher: a sheet type of the phase, and the route that opens its sheet. */
export type SheetTab = {
  id: number;
  name: string;
  icon: string;
  link: (string | number)[];
}
