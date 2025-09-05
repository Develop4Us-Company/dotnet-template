using System;
using System.ComponentModel.DataAnnotations;

namespace AppProject.Models;

public class GetByParentIdRequest<TIdType> : IRequest
{
    [Required]
    required public TIdType ParentId { get; set; }
}
