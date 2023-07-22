using Base.Core.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Base.Core.Application;

namespace Base.Infrastructure.Interceptors;

public sealed class UpdateAuditableEntitiesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public UpdateAuditableEntitiesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if(dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        IEnumerable<EntityEntry<AuditableEntity>> entries = dbContext.ChangeTracker.Entries<AuditableEntity>();

        foreach (EntityEntry<AuditableEntity> entityEntry in entries)
        {
            if(entityEntry.State == EntityState.Added)
            {
                entityEntry.Entity.CreatedBy = _currentUserService.UserId;
                entityEntry.Entity.CreatedAt = DateTime.UtcNow;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
