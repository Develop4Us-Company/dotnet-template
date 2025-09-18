using System;
using System.ComponentModel.DataAnnotations;

namespace AppProject.Models;

public class DeleteRequest<TIdType> : IRequest
{
    [Required]
    required public TIdType Id { get; set; }
}
