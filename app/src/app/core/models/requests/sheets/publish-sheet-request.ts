import { User } from '../../domain/access/user';

export type PublishSheetRequest = {
  id: number;
  changeLog: string;
  approvers: User[];
};
