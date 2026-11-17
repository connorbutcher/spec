import { User } from './user';

export type SpecSheetPhaseObserver = {
  id: number;
  specSheetPhaseId: number;
  userId: number;
  isApprover: boolean;
  user: User;
};
