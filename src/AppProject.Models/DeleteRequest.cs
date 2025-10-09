using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models.CustomValidators;

namespace AppProject.Models;

public class DeleteRequest<TIdType> : IRequest
{
    [RequiredGuid]
    required public TIdType Id { get; set; }
}
