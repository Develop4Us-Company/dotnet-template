using System;
using System.ComponentModel.DataAnnotations;

namespace AppProject.Models;

public class DeleteRequest<TIdType> : IRequest
{
    [Required]
    public required TIdType Id { get; set; }
}
