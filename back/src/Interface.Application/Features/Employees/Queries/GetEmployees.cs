using AutoMapper;
using FluentValidation;
using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Models;
using Interface.Domain.Employees;
using MediatR;

namespace Interface.Application.Features.Employees.Queries;

/// <summary>Paged employee list. <see cref="Search"/> matches name or email; the other filters are exact.</summary>
public sealed record GetEmployeesQuery : IRequest<PagedResult<EmployeeDto>>
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public string? Search { get; init; }
    public string? Department { get; init; }
    public EmployeeStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = DefaultPageSize;
}

public sealed class GetEmployeesQueryValidator : AbstractValidator<GetEmployeesQuery>
{
    public GetEmployeesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetEmployeesQuery.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(Employee.FullNameMaxLength);
        RuleFor(x => x.Department).MaximumLength(Employee.DepartmentMaxLength);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class GetEmployeesQueryHandler(
    IEmployeeRepository employees,
    IMapper mapper) : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    public async Task<PagedResult<EmployeeDto>> Handle(GetEmployeesQuery query, CancellationToken cancellationToken)
    {
        var criteria = new EmployeeSearchCriteria(
            query.Search?.Trim(),
            query.Department?.Trim(),
            query.Status,
            query.Page,
            query.PageSize);

        var page = await employees.SearchAsync(criteria, cancellationToken);
        return page.Map(mapper.Map<EmployeeDto>);
    }
}
