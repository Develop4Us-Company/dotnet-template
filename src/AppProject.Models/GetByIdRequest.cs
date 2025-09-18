using System;
using System.ComponentModel.DataAnnotations;

namespace AppProject.Models;

public class GetByIdRequest<TIdType> : IRequest
{
    [Required]
    public required TIdType Id { get; set; }
}
