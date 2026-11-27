import { HttpErrorResponse } from '@angular/common/http';

import { toApiError } from './api-error';

describe('toApiError', () => {
  it('keeps field errors from a 400 validation Problem Details', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        title: 'One or more validation errors occurred.',
        status: 400,
        detail: 'See the errors property for details.',
        errors: { email: ['Email must be a valid email address.'] },
      },
    });

    const apiError = toApiError(error);

    expect(apiError.status).toBe(400);
    expect(apiError.fieldErrors).toEqual({ email: ['Email must be a valid email address.'] });
    expect(apiError.message).toBe('One or more validation errors occurred.');
  });

  it('uses the detail and code of a business error', () => {
    const error = new HttpErrorResponse({
      status: 409,
      error: { title: 'Conflict', status: 409, detail: 'Email already exists.', code: 'employee.email_taken' },
    });

    const apiError = toApiError(error);

    expect(apiError.message).toBe('Email already exists.');
    expect(apiError.code).toBe('employee.email_taken');
    expect(apiError.fieldErrors).toEqual({});
  });

  it('explains when the server cannot be reached', () => {
    const apiError = toApiError(new HttpErrorResponse({ status: 0 }));

    expect(apiError.status).toBe(0);
    expect(apiError.message).toContain('Cannot reach the server');
  });

  it('falls back to a generic message when the body is not Problem Details', () => {
    const apiError = toApiError(new HttpErrorResponse({ status: 500, error: 'boom' }));

    expect(apiError.message).toBe('The server had a problem. Please try again later.');
  });
});
