using System;
using System.ComponentModel.DataAnnotations;

namespace AppProject.Models;

public class CreateOrUpdateRequest<TEntity> : IRequest
    where TEntity : class, IEntity
{
    [Required]
    required public TEntity Entity { get; set; }
}
