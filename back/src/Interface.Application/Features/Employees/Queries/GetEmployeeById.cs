using AutoMapper;
using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using Interface.Domain.Employees;
using MediatR;

namespace Interface.Application.Features.Employees.Queries;

public sealed record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto>;

public sealed class GetEmployeeByIdQueryHandler(
    IEmployeeRepository employees,
    IMapper mapper) : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery query, CancellationToken cancellationToken)
    {
        var employee = await employees.GetByIdAsync(query.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Employee), query.Id);

        return mapper.Map<EmployeeDto>(employee);
    }
}
