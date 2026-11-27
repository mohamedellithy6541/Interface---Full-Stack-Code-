import { HttpErrorResponse } from '@angular/common/http';

/** The API's RFC 7807 Problem Details body, plus the extension members our backend adds. */
interface ProblemDetailsBody {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
  errors?: Record<string, string[]>;
}

/** An HTTP failure in a form the UI can show directly. */
export interface ApiError {
  status: number;
  title: string;
  message: string;
  /** Machine-readable business error, e.g. "employee.email_taken". */
  code?: string;
  /** Validation messages per field, keyed by the API's camelCase path, e.g. "allowances[0].amount". */
  fieldErrors: Record<string, string[]>;
}

export function toApiError(error: unknown): ApiError {
  if (!(error instanceof HttpErrorResponse)) {
    return { status: -1, title: 'Unexpected error', message: 'Something went wrong. Please try again.', fieldErrors: {} };
  }

  if (error.status === 0) {
    return {
      status: 0,
      title: 'Server unreachable',
      message: 'Cannot reach the server. Check that the API is running and try again.',
      fieldErrors: {},
    };
  }

  const problem = isProblemDetails(error.error) ? error.error : {};
  const fieldErrors = problem.errors ?? {};

  return {
    status: error.status,
    title: problem.title ?? error.statusText ?? 'Request failed',
    message: problem.detail && Object.keys(fieldErrors).length === 0
      ? problem.detail
      : problem.title ?? defaultMessage(error.status),
    code: problem.code,
    fieldErrors,
  };
}

function isProblemDetails(body: unknown): body is ProblemDetailsBody {
  return typeof body === 'object' && body !== null && ('title' in body || 'status' in body);
}

function defaultMessage(status: number): string {
  switch (status) {
    case 401: return 'Please sign in again.';
    case 403: return 'You do not have permission to do this.';
    case 404: return 'The requested item was not found.';
    default: return status >= 500 ? 'The server had a problem. Please try again later.' : 'The request failed.';
  }
}
