const STORAGE_KEY = 'pu-spec-sheet.developerUser';
const QUERY_PARAMETER = 'developerUser';

/**
 * Development only, until sign-in exists: the user name this browser tab runs as, or null for the
 * default developer user. Open the app with `?developerUser=engineer2` to switch a tab to that user
 * (kept for the life of the tab), or `?developerUser=` to switch back. The API only honours it when
 * its DeveloperSignIn:AllowUserSwitching setting is on.
 */
function readDeveloperUser(): string | null {
  try {
    const requested = new URLSearchParams(window.location.search).get(QUERY_PARAMETER);
    if (requested === '') {
      window.sessionStorage.removeItem(STORAGE_KEY);
    } else if (requested !== null) {
      window.sessionStorage.setItem(STORAGE_KEY, requested);
    }
    return window.sessionStorage.getItem(STORAGE_KEY);
  } catch {
    return null;
  }
}

/** Read once, as the app loads, before the router has a chance to drop the query string. */
export const DEVELOPER_USER: string | null = readDeveloperUser();

export const DEVELOPER_USER_QUERY_PARAMETER = QUERY_PARAMETER;
