import { CreateTablePermissionRequest } from './create-table-permission-request';

export type ManageTablePermissionsRequest = {
  tableReferenceId: string;
  createTablePermissionRequests: CreateTablePermissionRequest[];
  deleteTablePermissionRequests: number[];
};
