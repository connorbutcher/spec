export type SheetExternalReference = {
  id: number;
  sheetId: number;
  referenceId: string;
  externalId: number;
  systemName: string;
  ownerName: string;
  url: string;
  type: string;
  grouping?: string;
  systemIconUrl?: string;
  description?: string;
};
