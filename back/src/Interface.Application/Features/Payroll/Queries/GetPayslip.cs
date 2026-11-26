using AutoMapper;
using FluentValidation;
using Interface.Application.Abstractions.Authentication;
using Interface.Application.Abstractions.Payroll;
using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using Interface.Domain.Employees;
using Interface.Domain.Payroll;
using MediatR;

namespace Interface.Application.Features.Payroll.Queries;

/// <param name="Month">The payroll month as "yyyy-MM", e.g. "2026-03".</param>
public sealed record GetPayslipQuery(Guid EmployeeId, string Month) : IRequest<PayslipDto>;

public sealed class GetPayslipQueryValidator : AbstractValidator<GetPayslipQuery>
{
    public GetPayslipQueryValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Month)
            .NotEmpty().WithMessage("Month is required.")
            .Must(month => PayrollPeriod.TryParse(month, out _))
            .WithMessage($"Month must use the format {PayrollPeriod.Format}, e.g. 2026-03.");
    }
}

public sealed class GetPayslipQueryHandler(
    IEmployeeRepository employees,
    IPayrollPolicyProvider policyProvider,
    IPayrollCalculator payrollCalculator,
    ICurrentUser currentUser,
    IMapper mapper) : IRequestHandler<GetPayslipQuery, PayslipDto>
{
    public async Task<PayslipDto> Handle(GetPayslipQuery query, CancellationToken cancellationToken)
    {
        EnsureCallerMayViewPayslip(query.EmployeeId);

        var employee = await employees.GetByIdAsync(query.EmployeeId, cancellationToken)
                       ?? throw new NotFoundException(nameof(Employee), query.EmployeeId);

        var payslip = payrollCalculator.Calculate(
            employee,
            PayrollPeriod.Parse(query.Month),
            policyProvider.GetCurrentPolicy());

        return mapper.Map<PayslipDto>(payslip);
    }

    // HR can see every payslip; an employee only their own. Checked before loading the employee,
    // so an employee cannot probe which employee ids exist.
    private void EnsureCallerMayViewPayslip(Guid employeeId)
    {
        if (currentUser.IsHr || currentUser.EmployeeId == employeeId)
            return;

        throw new ForbiddenAccessException("You can only view your own payslip.");
    }
}
