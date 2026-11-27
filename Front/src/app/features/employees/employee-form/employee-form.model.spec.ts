import { EmployeeDto } from '../../../core/api/generated/models';
import { createEmployeeForm, createPayComponentForm, fillEmployeeForm, toEmployeeRequest } from './employee-form.model';

describe('employee form model', () => {
  const employee: EmployeeDto = {
    id: '6747ec31-29dd-41f3-aae9-689f2f07689e',
    fullName: 'Sara Ali',
    email: 'sara.ali@interface.local',
    jobTitle: 'Software Engineer',
    department: 'Engineering',
    hireDate: '2023-11-26',
    baseMonthlySalary: 25000,
    status: 'Active',
    allowances: [
      { name: 'Transport', amount: 1500 },
      { name: 'Housing', amount: 3000 },
    ],
    deductions: [{ name: 'Medical plan', amount: 400 }],
  };

  it('starts invalid, with every required field reported', () => {
    const form = createEmployeeForm();

    expect(form.valid).toBe(false);
    expect(form.controls.fullName.hasError('required')).toBe(true);
    expect(form.controls.baseMonthlySalary.hasError('required')).toBe(true);
    expect(form.controls.status.value).toBe('Active');
  });

  it('loads an employee, including allowance and deduction rows', () => {
    const form = createEmployeeForm();

    fillEmployeeForm(form, employee);

    expect(form.valid).toBe(true);
    expect(form.controls.allowances.length).toBe(2);
    expect(form.controls.deductions.at(0).value).toEqual({ name: 'Medical plan', amount: 400 });
  });

  it('builds the API request with trimmed text and numeric amounts', () => {
    const form = createEmployeeForm();
    fillEmployeeForm(form, employee);
    form.controls.fullName.setValue('  Sara Ali  ');
    form.controls.allowances.at(0).controls.name.setValue(' Transport ');

    const request = toEmployeeRequest(form.getRawValue());

    expect(request.fullName).toBe('Sara Ali');
    expect(request.allowances[0]).toEqual({ name: 'Transport', amount: 1500 });
    expect(request.baseMonthlySalary).toBe(25000);
  });

  it('rejects a full name longer than 100 characters', () => {
    const form = createEmployeeForm();
    form.controls.fullName.setValue('a'.repeat(101));

    expect(form.controls.fullName.hasError('maxlength')).toBe(true);
  });

  it('rejects a negative allowance', () => {
    const row = createPayComponentForm({ name: 'Bonus', amount: -1 });

    expect(row.controls.amount.hasError('min')).toBe(true);
  });
});
