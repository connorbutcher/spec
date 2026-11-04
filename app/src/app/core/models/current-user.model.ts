/** The user every request runs as, with what their roles allow. */
export type CurrentUser = {
  id: number;
  userName: string;
  displayName: string;
  isAdministrator: boolean;
  roles: string[];
  /** Permission keys the user holds, such as `phases.manage`. An administrator holds every one. */
  permissions: string[];
}
