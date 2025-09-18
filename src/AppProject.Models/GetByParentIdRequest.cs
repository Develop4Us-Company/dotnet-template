using System;
using System.ComponentModel.DataAnnotations;

namespace AppProject.Models;

public class GetByParentIdRequest<TIdType> : IRequest
{
    [Required]
    public required TIdType ParentId { get; set; }
}
