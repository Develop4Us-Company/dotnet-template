using System;
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.Database.Entities;
using AppProject.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace AppProject.Core.Infrastructure.Database;

public class DatabaseRepository(
    ApplicationDbContext applicationDbContext,
    IUserContext userContext)
    : IDatabaseRepository
{
    public async Task InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
    where TEntity : BaseEntity
    {
        await this.SetAuditFieldsAsync(entity, isInsert: true);
        await applicationDbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity
    {
        await this.SetAuditFieldsAsync(entity, isInsert: false);
        applicationDbContext.Set<TEntity>().Update(entity);
    }

    public Task DeleteAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity
    {
        applicationDbContext.Set<TEntity>().Remove(entity);

        return Task.CompletedTask;
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException concurrencyException)
        {
            throw new AppException(ExceptionCode.Concurrency, innerException: concurrencyException);
        }
    }

    public async Task<IList<TDestination>> GetAllAsync<TEntity, TDestination>(CancellationToken cancellationToken)
        where TEntity : BaseEntity
        where TDestination : class
    {
        return await applicationDbContext.Set<TEntity>().AsQueryable().ProjectToType<TDestination>().ToListAsync(cancellationToken);
    }

    public async Task<IList<TEntity>> GetAllAsync<TEntity>(CancellationToken cancellationToken)
        where TEntity : BaseEntity
    {
        return await applicationDbContext.Set<TEntity>().AsQueryable().ToListAsync(cancellationToken);
    }

    public async Task<IList<TDestination>> GetByConditionAsync<TEntity, TDestination>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken)
        where TEntity : BaseEntity
        where TDestination : class
    {
        return await queryable(applicationDbContext.Set<TEntity>().AsQueryable()).ProjectToType<TDestination>().ToListAsync(cancellationToken);
    }

    public async Task<IList<TEntity>> GetByConditionAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken)
        where TEntity : BaseEntity
    {
        return await queryable(applicationDbContext.Set<TEntity>().AsQueryable()).ToListAsync(cancellationToken);
    }

    private async Task SetAuditFieldsAsync<TEntity>(TEntity entity, bool isInsert)
        where TEntity : BaseEntity
    {
        var currentUser = await userContext.GetCurrentUserAsync();

        var now = DateTime.UtcNow;

        if (isInsert)
        {
            entity.CreatedAt = now;
            entity.CreatedByUserName = currentUser.UserName;
            entity.CreatedByUserId = currentUser.UserId;
        }

        entity.UpdatedAt = now;
        entity.UpdatedByUserName = currentUser.UserName;
        entity.CreatedByUserId = currentUser.UserId;
    }
}
