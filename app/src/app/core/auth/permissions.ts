/** The permission keys the API checks. Each matches a row in its Permissions table. */
export const PERMISSIONS = {
  phasesManage: 'phases.manage',
  sheetTypesManage: 'sheetTypes.manage',
} as const;

export type Permission = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];
