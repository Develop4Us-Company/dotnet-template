using System;

namespace AppProject.Models;

public class EntitiesResponse<TEntity> : IResponse
    where TEntity : class, IEntity
{
    required public IReadOnlyCollection<TEntity> Entities { get; set; }
}
