using FluentValidation;
using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using Interface.Domain.Employees;
using MediatR;

namespace Interface.Application.Features.Employees.Commands;

public sealed record DeleteEmployeeCommand(Guid Id) : IRequest;

public sealed class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class DeleteEmployeeCommandHandler(
    IEmployeeRepository employees,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteEmployeeCommand>
{
    public async Task Handle(DeleteEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await employees.GetByIdAsync(command.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Employee), command.Id);

        employees.Remove(employee);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
