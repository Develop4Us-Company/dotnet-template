using System;

namespace AppProject.Models;

public class EntitiesResponse<TEntity> : IResponse
    where TEntity : class, IEntity
{
    public IReadOnlyCollection<TEntity> Entities { get; set; } = Array.Empty<TEntity>();
}
