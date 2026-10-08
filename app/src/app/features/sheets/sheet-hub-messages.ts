/**
 * The names the browser and the API's `SheetHub` use for each other's messages. They must match the
 * method names on `SheetHub` and `ISheetHubClient` in the API (`Api/Collaboration`).
 */
export const SHEET_HUB = {
  /** Where the hub is served. The dev proxy forwards it to the API. */
  url: '/hubs/sheets',
  /** The call the browser makes. Joining another sheet leaves the one before. */
  joinSheet: 'JoinSheet',
  /** Messages the server sends. */
  presenceChanged: 'PresenceChanged',
  sheetChanged: 'SheetChanged',
  takeoverChanged: 'TakeoverChanged',
} as const;
