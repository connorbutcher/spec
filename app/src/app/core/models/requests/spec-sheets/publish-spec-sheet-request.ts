import { User } from '../../domain/access/user';

export type PublishSpecSheetRequest = {
  id: number;
  changeLog: string;
  approvers: User[];
};
