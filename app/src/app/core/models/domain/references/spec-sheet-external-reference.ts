export type SpecSheetExternalReference = {
  id: number;
  specSheetId: number;
  externalId: number;
  referenceId: string;
  systemName: string;
  ownerName: string;
  url: string;
  type: string;
  grouping?: string;
  systemIconUrl?: string;
  description?: string;
};
