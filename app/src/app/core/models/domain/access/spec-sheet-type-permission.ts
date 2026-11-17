import { Permission } from './permission';

export type SpecSheetTypePermission = {
  id: number;
  permissionId: number;
  specSheetTypeId: number;
  permission?: Permission;
};
