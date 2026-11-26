using AutoMapper;
using FluentValidation;
using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using Interface.Domain.Employees;
using MediatR;

namespace Interface.Application.Features.Employees.Commands;

public sealed record UpdateEmployeeCommand : IRequest<EmployeeDto>, IEmployeeInput
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public DateOnly HireDate { get; init; }
    public decimal BaseMonthlySalary { get; init; }
    public EmployeeStatus Status { get; init; }
    public IReadOnlyList<PayComponentDto> Allowances { get; init; } = [];
    public IReadOnlyList<PayComponentDto> Deductions { get; init; } = [];
}

public sealed class UpdateEmployeeCommandValidator : EmployeeInputValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator(TimeProvider timeProvider) : base(timeProvider)
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class UpdateEmployeeCommandHandler(
    IEmployeeRepository employees,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await employees.GetByIdAsync(command.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Employee), command.Id);

        if (await employees.EmailExistsAsync(command.Email, excludingEmployeeId: employee.Id, cancellationToken))
            throw EmployeeErrors.EmailTaken(command.Email);

        employee.UpdateDetails(
            command.FullName,
            command.Email,
            command.JobTitle,
            command.Department,
            command.HireDate,
            command.BaseMonthlySalary,
            command.Status);
        employee.ReplaceAllowances(mapper.Map<IEnumerable<PayComponent>>(command.Allowances));
        employee.ReplaceDeductions(mapper.Map<IEnumerable<PayComponent>>(command.Deductions));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<EmployeeDto>(employee);
    }
}
