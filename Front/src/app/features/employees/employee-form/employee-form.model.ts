import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';

import { EmployeeDto, EmployeeRequest, EmployeeStatus, PayComponentDto } from '../../../core/api/generated/models';
import { greaterThan, maxDecimalPlaces, notBlank, notInFuture } from '../../../shared/forms/validators';

/** Same limits as the API (Employee.cs / PayComponentValidator.cs), so the UI catches mistakes first. */
export const EmployeeLimits = {
  fullName: 100,
  email: 256,
  jobTitle: 100,
  department: 100,
  payComponentName: 100,
  moneyDecimals: 2,
} as const;

export type PayComponentForm = FormGroup<{
  name: FormControl<string>;
  amount: FormControl<number | null>;
}>;

export type EmployeeForm = FormGroup<{
  fullName: FormControl<string>;
  email: FormControl<string>;
  jobTitle: FormControl<string>;
  department: FormControl<string>;
  hireDate: FormControl<string>;
  baseMonthlySalary: FormControl<number | null>;
  status: FormControl<EmployeeStatus>;
  allowances: FormArray<PayComponentForm>;
  deductions: FormArray<PayComponentForm>;
}>;

export function createEmployeeForm(): EmployeeForm {
  return new FormGroup({
    fullName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, notBlank, Validators.maxLength(EmployeeLimits.fullName)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(EmployeeLimits.email)],
    }),
    jobTitle: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, notBlank, Validators.maxLength(EmployeeLimits.jobTitle)],
    }),
    department: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, notBlank, Validators.maxLength(EmployeeLimits.department)],
    }),
    hireDate: new FormControl('', { nonNullable: true, validators: [Validators.required, notInFuture()] }),
    baseMonthlySalary: new FormControl<number | null>(null, {
      validators: [Validators.required, greaterThan(0), maxDecimalPlaces(EmployeeLimits.moneyDecimals)],
    }),
    status: new FormControl<EmployeeStatus>('Active', { nonNullable: true, validators: [Validators.required] }),
    allowances: new FormArray<PayComponentForm>([]),
    deductions: new FormArray<PayComponentForm>([]),
  });
}

export function createPayComponentForm(value?: PayComponentDto): PayComponentForm {
  return new FormGroup({
    name: new FormControl(value?.name ?? '', {
      nonNullable: true,
      validators: [Validators.required, notBlank, Validators.maxLength(EmployeeLimits.payComponentName)],
    }),
    amount: new FormControl<number | null>(value?.amount ?? null, {
      validators: [Validators.required, Validators.min(0), maxDecimalPlaces(EmployeeLimits.moneyDecimals)],
    }),
  });
}

/** Loads an existing employee into the form, rebuilding the allowance and deduction rows. */
export function fillEmployeeForm(form: EmployeeForm, employee: EmployeeDto): void {
  form.controls.allowances.clear();
  employee.allowances.forEach((item) => form.controls.allowances.push(createPayComponentForm(item)));
  form.controls.deductions.clear();
  employee.deductions.forEach((item) => form.controls.deductions.push(createPayComponentForm(item)));

  form.patchValue({
    fullName: employee.fullName,
    email: employee.email,
    jobTitle: employee.jobTitle,
    department: employee.department,
    hireDate: employee.hireDate,
    baseMonthlySalary: employee.baseMonthlySalary,
    status: employee.status,
  });
}

export function toEmployeeRequest(value: ReturnType<EmployeeForm['getRawValue']>): EmployeeRequest {
  return {
    fullName: value.fullName.trim(),
    email: value.email.trim(),
    jobTitle: value.jobTitle.trim(),
    department: value.department.trim(),
    hireDate: value.hireDate,
    baseMonthlySalary: Number(value.baseMonthlySalary),
    status: value.status,
    allowances: value.allowances.map(toPayComponent),
    deductions: value.deductions.map(toPayComponent),
  };
}

function toPayComponent(item: { name: string; amount: number | null }): PayComponentDto {
  return { name: item.name.trim(), amount: Number(item.amount) };
}
