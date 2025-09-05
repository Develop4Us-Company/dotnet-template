using System;
using System.ComponentModel.DataAnnotations;

namespace AppProject.Models;

public class GetByIdRequest<TIdType> : IRequest
{
    [Required]
    public TIdType Id { get; set; }
}
