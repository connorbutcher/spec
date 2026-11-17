import { SpecSheetTablePermission } from './spec-sheet-table-permission';
import { UserPermission } from './user-permission';

export type User = {
  id: number;
  objectId: string;
  displayName: string;
  emailAddress: string;
  permissions: UserPermission[];
  tablePermissions: SpecSheetTablePermission[];
};
