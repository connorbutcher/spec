import { HttpErrorResponse } from '@angular/common/http';

/** A readable message from a failed API call: the ProblemDetails detail, the first validation error, or a fallback. */
export function apiErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Something went wrong. Please try again.';
  }
  if (error.status === 0) {
    return "Couldn't reach the API. Check it's running, then try again.";
  }

  const body: unknown = error.error;
  if (body && typeof body === 'object') {
    const problem = body as { detail?: unknown; errors?: Record<string, unknown>; title?: unknown };
    if (typeof problem.detail === 'string' && problem.detail) {
      return problem.detail;
    }
    const firstError = Object.values(problem.errors ?? {}).flat()[0];
    if (typeof firstError === 'string') {
      return firstError;
    }
    if (typeof problem.title === 'string' && problem.title) {
      return problem.title;
    }
  }
  return `The change couldn't be saved (${error.status}).`;
}
