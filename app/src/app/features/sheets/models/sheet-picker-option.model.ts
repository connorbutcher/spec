/** One of the phase's sheets of a type in the sheet picker: its name and the route that opens it. */
export type SheetPickerOption = {
  id: number;
  label: string;
  link: (string | number)[];
}
