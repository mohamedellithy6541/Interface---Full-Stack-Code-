import { FormArray, FormControl, FormGroup } from '@angular/forms';

import { applyServerErrors, toFormPath } from './server-errors';

describe('applyServerErrors', () => {
  function createForm() {
    return new FormGroup({
      email: new FormControl(''),
      allowances: new FormArray([new FormGroup({ name: new FormControl('Transport'), amount: new FormControl(-1) })]),
    });
  }

  it('converts API paths to form paths', () => {
    expect(toFormPath('allowances[0].amount')).toBe('allowances.0.amount');
    expect(toFormPath('email')).toBe('email');
  });

  it('puts each message on its control, including nested array items', () => {
    const form = createForm();

    const unmatched = applyServerErrors(form, {
      email: ['Email is taken.'],
      'allowances[0].amount': ['Amount cannot be negative.'],
    });

    expect(unmatched).toEqual([]);
    expect(form.get('email')?.errors).toEqual({ server: 'Email is taken.' });
    expect(form.get('allowances.0.amount')?.errors).toEqual({ server: 'Amount cannot be negative.' });
    expect(form.get('email')?.touched).toBe(true);
  });

  it('returns messages for fields the form does not have', () => {
    const unmatched = applyServerErrors(createForm(), { id: ['Id is required.'] });

    expect(unmatched).toEqual(['Id is required.']);
  });

  it('clears the server error once the user edits the field', () => {
    const form = createForm();
    applyServerErrors(form, { email: ['Email is taken.'] });

    form.controls.email.setValue('new@interface.local');

    expect(form.controls.email.errors).toBeNull();
  });
});
