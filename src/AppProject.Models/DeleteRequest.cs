using System;
using System.ComponentModel.DataAnnotations;

namespace AppProject.Models;

public class DeleteRequest<TIdType> : IRequest
{
    [Required]
    public TIdType Id { get; set; } = default!;
}
