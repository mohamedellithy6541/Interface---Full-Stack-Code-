import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Today's date as "yyyy-MM-dd" in the user's local time zone. */
export function todayIsoDate(now: Date = new Date()): string {
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${now.getFullYear()}-${month}-${day}`;
}

/** The current month as "yyyy-MM". */
export function currentIsoMonth(now: Date = new Date()): string {
  return todayIsoDate(now).slice(0, 7);
}

/** Validators.required accepts "   "; the API does not, so neither do we. */
export const notBlank: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value;
  return typeof value === 'string' && value.length > 0 && value.trim().length === 0 ? { required: true } : null;
};

/** Fails when a "yyyy-MM-dd" date is after today. Mirrors the API rule "Hire date cannot be in the future". */
export function notInFuture(today: () => string = todayIsoDate): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as string | null;
    return value && value > today() ? { futureDate: true } : null;
  };
}

/** Fails unless the number is strictly greater than the limit (Validators.min is inclusive). */
export function greaterThan(limit: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (value === null || value === undefined || value === '') return null;
    return Number(value) > limit ? null : { greaterThan: { limit } };
  };
}

/** Fails when a number has more decimal places than money allows. */
export function maxDecimalPlaces(places: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (value === null || value === undefined || value === '') return null;
    const decimals = String(value).split('.')[1] ?? '';
    return decimals.length > places ? { decimalPlaces: { places } } : null;
  };
}
