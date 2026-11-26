using Interface.Domain.Payroll;

namespace Interface.Application.Abstractions.Payroll;

public interface IPayrollPolicyProvider
{
    PayrollPolicy GetCurrentPolicy();
}
