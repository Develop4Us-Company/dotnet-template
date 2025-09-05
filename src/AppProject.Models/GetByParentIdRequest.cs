using System;
using System.ComponentModel.DataAnnotations;

namespace AppProject.Models;

public class GetByParentIdRequest<TIdType> : IRequest
{
    [Required]
    public TIdType ParentId { get; set; }
}
