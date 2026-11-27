/** Business error codes the API returns in the Problem Details "code" member. */
export const ApiErrorCodes = {
  emailTaken: 'employee.email_taken',
  employeeInactive: 'payroll.employee_inactive',
  periodBeforeHireDate: 'payroll.period_before_hire_date',
  deductionsExceedGrossPay: 'payroll.deductions_exceed_gross_pay',
} as const;
