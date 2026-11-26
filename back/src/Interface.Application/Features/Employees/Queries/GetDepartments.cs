using Interface.Application.Abstractions.Persistence;
using MediatR;

namespace Interface.Application.Features.Employees.Queries;

/// <summary>Distinct departments in use, for the department filter in the UI.</summary>
public sealed record GetDepartmentsQuery : IRequest<IReadOnlyList<string>>;

public sealed class GetDepartmentsQueryHandler(IEmployeeRepository employees)
    : IRequestHandler<GetDepartmentsQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(GetDepartmentsQuery query, CancellationToken cancellationToken) =>
        employees.GetDepartmentsAsync(cancellationToken);
}
