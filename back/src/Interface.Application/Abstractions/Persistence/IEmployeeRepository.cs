using Interface.Application.Common.Models;
using Interface.Domain.Employees;

namespace Interface.Application.Abstractions.Persistence;

public sealed record EmployeeSearchCriteria(
    string? Search,
    string? Department,
    EmployeeStatus? Status,
    int Page,
    int PageSize);

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, Guid? excludingEmployeeId, CancellationToken cancellationToken);
    Task<PagedResult<Employee>> SearchAsync(EmployeeSearchCriteria criteria, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetDepartmentsAsync(CancellationToken cancellationToken);
    void Add(Employee employee);
    void Remove(Employee employee);
}
