import { SystemReferenceGroup } from './system-reference-group';

export type SystemReference = {
  system: string;
  iconUrl?: string;
  referenceGroups: SystemReferenceGroup[];
};
