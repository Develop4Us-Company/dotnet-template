using System;

namespace AppProject.Models;

public class EntityResponse<TEntity> : IResponse
    where TEntity : class, IEntity
{
    public TEntity Entity { get; set; }
}
