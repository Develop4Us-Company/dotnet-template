using System;
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.Database.Entities;
using AppProject.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppProject.Core.Infrastructure.Database;

public class DatabaseRepository(
    ApplicationDbContext applicationDbContext,
    IUserContext userContext,
    TypeAdapterConfig typeAdapterConfig)
    : IDatabaseRepository
{
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await applicationDbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
    where TEntity : BaseEntity
    {
        await this.SetAuditFieldsAsync(entity, isInsert: true);
        await applicationDbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public async Task InsertAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
    where TEntity : BaseEntity
    {
        await this.InsertAsync(entity, cancellationToken);
        await this.SaveAsync(cancellationToken);
    }

    public async Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        await this.SetAuditFieldsAsync(entity, isInsert: false, cancellationToken);
        applicationDbContext.Set<TEntity>().Update(entity);
    }

    public async Task UpdateAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        await this.UpdateAsync(entity, cancellationToken);
        await this.SaveAsync(cancellationToken);
    }

    public Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        applicationDbContext.Set<TEntity>().Remove(entity);

        return Task.CompletedTask;
    }

    public async Task DeleteAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        await this.DeleteAsync(entity, cancellationToken);
        await this.SaveAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
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

    public async Task<IReadOnlyCollection<TDestination>> GetAllAsync<TEntity, TDestination>(CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
        where TDestination : class
    {
        return await applicationDbContext.Set<TEntity>().AsQueryable().ProjectToType<TDestination>(typeAdapterConfig).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync<TEntity>(CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        return await applicationDbContext.Set<TEntity>().AsQueryable().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TDestination>> GetByConditionAsync<TEntity, TDestination>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
        where TDestination : class
    {
        return await queryable(applicationDbContext.Set<TEntity>().AsQueryable()).ProjectToType<TDestination>(typeAdapterConfig).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TEntity>> GetByConditionAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        return await queryable(applicationDbContext.Set<TEntity>().AsQueryable()).ToListAsync(cancellationToken);
    }

    public async Task<TDestination?> GetFirstOrDefaultAsync<TEntity, TDestination>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
        where TDestination : class
    {
        return (await this.GetByConditionAsync<TEntity, TDestination>(queryable, cancellationToken))
            .FirstOrDefault();
    }

    public async Task<TEntity?> GetFirstOrDefaultAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        return (await this.GetByConditionAsync(queryable, cancellationToken))
            .FirstOrDefault();
    }

    public async Task<bool> HasAnyAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        return await queryable(applicationDbContext.Set<TEntity>().AsQueryable()).AnyAsync(cancellationToken);
    }

    private async Task SetAuditFieldsAsync<TEntity>(TEntity entity, bool isInsert, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        var currentUser = await userContext.GetCurrentUserAsync(cancellationToken);

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
