import { Permission } from './permission';

export type UserPermission = {
  id: number;
  userId: number;
  permissionId: number;
  permission: Permission;
};
