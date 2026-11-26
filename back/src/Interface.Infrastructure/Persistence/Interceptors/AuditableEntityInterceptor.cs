using Interface.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Interface.Infrastructure.Persistence.Interceptors;

/// <summary>Stamps created/updated times on every <see cref="IAuditable"/> entity when changes are saved.</summary>
internal sealed class AuditableEntityInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            StampAuditFields(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void StampAuditFields(DbContext context)
    {
        var now = timeProvider.GetUtcNow();

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAtUtc = now;
            else if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                entry.Entity.UpdatedAtUtc = now;
        }
    }
}

internal static class EntityEntryExtensions
{
    public static bool HasChangedOwnedEntities(this Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) =>
        entry.References.Any(reference =>
            reference.TargetEntry is not null
            && reference.TargetEntry.Metadata.IsOwned()
            && reference.TargetEntry.State is EntityState.Added or EntityState.Modified);
}
