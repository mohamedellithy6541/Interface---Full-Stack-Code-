using Interface.Application.Abstractions.Persistence;
using Interface.Domain.Employees;
using Interface.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Interface.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = Employee.NormalizeEmail(email);
        return dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }
}
