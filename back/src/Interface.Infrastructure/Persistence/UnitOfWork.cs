using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Interface.Infrastructure.Persistence;

internal sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    private const int SqlServerUniqueIndexViolation = 2601;
    private const int SqlServerUniqueConstraintViolation = 2627;

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        // The handlers check uniqueness first; this covers two requests racing past that check.
        catch (DbUpdateException exception) when (exception.InnerException is SqlException
                                                  {
                                                      Number: SqlServerUniqueIndexViolation or SqlServerUniqueConstraintViolation
                                                  })
        {
            throw new ConflictException("persistence.duplicate", "A record with the same unique value already exists.");
        }
    }
}
