using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Models;
using Interface.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace Interface.Infrastructure.Persistence.Repositories;

internal sealed class EmployeeRepository(AppDbContext dbContext) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Employees.FirstOrDefaultAsync(employee => employee.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, Guid? excludingEmployeeId, CancellationToken cancellationToken)
    {
        var normalizedEmail = Employee.NormalizeEmail(email);
        return dbContext.Employees.AnyAsync(
            employee => employee.Email == normalizedEmail
                        && (excludingEmployeeId == null || employee.Id != excludingEmployeeId),
            cancellationToken);
    }

    public async Task<PagedResult<Employee>> SearchAsync(
        EmployeeSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search;
            query = query.Where(employee => employee.FullName.Contains(term) || employee.Email.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Department))
            query = query.Where(employee => employee.Department == criteria.Department);

        if (criteria.Status is { } status)
            query = query.Where(employee => employee.Status == status);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(employee => employee.FullName)
            .ThenBy(employee => employee.Id)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Employee>(items, criteria.Page, criteria.PageSize, totalCount);
    }

    public async Task<IReadOnlyList<string>> GetDepartmentsAsync(CancellationToken cancellationToken) =>
        await dbContext.Employees
            .Select(employee => employee.Department)
            .Distinct()
            .OrderBy(department => department)
            .ToListAsync(cancellationToken);

    public void Add(Employee employee) => dbContext.Employees.Add(employee);

    public void Remove(Employee employee) => dbContext.Employees.Remove(employee);
}
