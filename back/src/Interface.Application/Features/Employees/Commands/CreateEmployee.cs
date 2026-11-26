using AutoMapper;
using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using Interface.Domain.Employees;
using MediatR;

namespace Interface.Application.Features.Employees.Commands;

public sealed record CreateEmployeeCommand : IRequest<EmployeeDto>, IEmployeeInput
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public DateOnly HireDate { get; init; }
    public decimal BaseMonthlySalary { get; init; }
    public EmployeeStatus Status { get; init; } = EmployeeStatus.Active;
    public IReadOnlyList<PayComponentDto> Allowances { get; init; } = [];
    public IReadOnlyList<PayComponentDto> Deductions { get; init; } = [];
}

public sealed class CreateEmployeeCommandValidator(TimeProvider timeProvider)
    : EmployeeInputValidator<CreateEmployeeCommand>(timeProvider);

public sealed class CreateEmployeeCommandHandler(
    IEmployeeRepository employees,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (await employees.EmailExistsAsync(command.Email, excludingEmployeeId: null, cancellationToken))
            throw EmployeeErrors.EmailTaken(command.Email);

        var employee = Employee.Create(
            command.FullName,
            command.Email,
            command.JobTitle,
            command.Department,
            command.HireDate,
            command.BaseMonthlySalary,
            command.Status,
            mapper.Map<IEnumerable<PayComponent>>(command.Allowances),
            mapper.Map<IEnumerable<PayComponent>>(command.Deductions));

        employees.Add(employee);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<EmployeeDto>(employee);
    }
}

internal static class EmployeeErrors
{
    public const string EmailTakenCode = "employee.email_taken";

    public static ConflictException EmailTaken(string email) =>
        new(EmailTakenCode, $"An employee with email '{Employee.NormalizeEmail(email)}' already exists.");
}
