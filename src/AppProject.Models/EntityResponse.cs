using System;

namespace AppProject.Models;

public class EntityResponse<TEntity> : IResponse
    where TEntity : class, IEntity
{
    required public TEntity Entity { get; set; }
}
