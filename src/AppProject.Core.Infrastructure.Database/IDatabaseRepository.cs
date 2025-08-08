using System;
using AppProject.Core.Infrastructure.Database.Entities;

namespace AppProject.Core.Infrastructure.Database;

public interface IDatabaseRepository
{
    public Task InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
        where TEntity : BaseEntity;

    public Task UpdateAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity;

    public Task DeleteAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity;

    public Task SaveAsync(CancellationToken cancellationToken);

    public Task<IList<TDestination>> GetAllAsync<TEntity, TDestination>(CancellationToken cancellationToken)
        where TEntity : BaseEntity
        where TDestination : class;

    public Task<IList<TEntity>> GetAllAsync<TEntity>(CancellationToken cancellationToken)
        where TEntity : BaseEntity;

    public Task<IList<TDestination>> GetByConditionAsync<TEntity, TDestination>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken)
        where TEntity : BaseEntity
        where TDestination : class;

    public Task<IList<TEntity>> GetByConditionAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryable, CancellationToken cancellationToken)
        where TEntity : BaseEntity;
}
