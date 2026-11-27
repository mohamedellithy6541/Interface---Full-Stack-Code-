import { Component, input } from '@angular/core';
import { AbstractControl, ValidationErrors } from '@angular/forms';

/** Shows the first validation message of a control once the user has touched it. */
@Component({
  selector: 'app-field-error',
  template: `
    @if (message(); as text) {
      <p class="field-error" role="alert">{{ text }}</p>
    }
  `,
})
export class FieldError {
  readonly control = input.required<AbstractControl>();
  readonly label = input('This field');

  protected message(): string | null {
    const control = this.control();
    if (!control.errors || !(control.touched || control.dirty)) return null;
    return describeError(control.errors, this.label());
  }
}

export function describeError(errors: ValidationErrors, label: string): string {
  if (errors['server']) return errors['server'];
  if (errors['required']) return `${label} is required.`;
  if (errors['maxlength']) return `${label} must be at most ${errors['maxlength'].requiredLength} characters.`;
  if (errors['email']) return 'Enter a valid email address.';
  if (errors['futureDate']) return `${label} cannot be in the future.`;
  if (errors['greaterThan']) return `${label} must be greater than ${errors['greaterThan'].limit}.`;
  if (errors['min']) return `${label} cannot be less than ${errors['min'].min}.`;
  if (errors['decimalPlaces']) return `${label} can have at most ${errors['decimalPlaces'].places} decimal places.`;
  return `${label} is invalid.`;
}
