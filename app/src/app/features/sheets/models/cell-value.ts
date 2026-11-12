/**
 * A cell's value, in the shape its kind takes: text for text and linked dropdown cells, a number for number
 * cells, a `yyyy-MM-dd` date for date cells, true or false for checkboxes, the chosen option's id for text and
 * number dropdowns, and slot key to text for labelled text cells. Null when the cell holds nothing.
 */
export type CellValue = string | number | boolean | Readonly<Record<string, string>> | null;
