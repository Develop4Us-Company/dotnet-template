using System;

namespace AppProject.Models;

public class EntityResponse<TEntity> : IResponse
    where TEntity : class, IEntity
{
    public required TEntity Entity { get; set; }
}
