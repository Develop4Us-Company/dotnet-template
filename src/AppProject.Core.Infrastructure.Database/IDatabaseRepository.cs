using System;
using AppProject.Core.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppProject.Core.Infrastructure.Database;

public interface IDatabaseRepository
{
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    public Task InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task InsertAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task UpdateAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task DeleteAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task SaveAsync(CancellationToken cancellationToken = default);

    public Task<IList<TDestination>> GetAllAsync<TEntity, TDestination>(CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
        where TDestination : class;

    public Task<IList<TEntity>> GetAllAsync<TEntity>(CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task<IList<TDestination>> GetByConditionAsync<TEntity, TDestination>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
        where TDestination : class;

    public Task<IList<TEntity>> GetByConditionAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task<TDestination?> GetFirstOrDefaultAsync<TEntity, TDestination>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
        where TDestination : class;

    public Task<TEntity?> GetFirstOrDefaultAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task<bool> HasAnyAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;
}
