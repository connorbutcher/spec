export type UpdateUserPermissionsRequest = {
  userId: number;
  permissionIdsToAdd: number[];
  permissionIdsToRemove: number[];
};
