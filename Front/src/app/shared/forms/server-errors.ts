import { AbstractControl } from '@angular/forms';

/**
 * Puts the API's validation messages on the matching form controls, so each appears under its input.
 * API paths such as "allowances[0].amount" are converted to form paths ("allowances.0.amount").
 * Returns the messages for fields that have no control, so the caller can show them elsewhere.
 * A server error disappears as soon as the user edits the field, because the control re-validates.
 */
export function applyServerErrors(form: AbstractControl, fieldErrors: Record<string, string[]>): string[] {
  const unmatched: string[] = [];

  for (const [apiPath, messages] of Object.entries(fieldErrors)) {
    const control = form.get(toFormPath(apiPath));
    if (!control) {
      unmatched.push(...messages);
      continue;
    }
    control.setErrors({ ...control.errors, server: messages.join(' ') });
    control.markAsTouched();
  }

  return unmatched;
}

export function toFormPath(apiPath: string): string {
  return apiPath.replace(/\[(\d+)\]/g, '.$1');
}
