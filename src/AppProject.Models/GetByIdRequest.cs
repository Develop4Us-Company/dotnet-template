using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models.CustomValidators;

namespace AppProject.Models;

public class GetByIdRequest<TIdType> : IRequest
{
    [RequiredGuid]
    required public TIdType Id { get; set; }
}
